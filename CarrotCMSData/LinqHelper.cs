using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

// portions derived from https://terryaney.wordpress.com/2008/04/14/batch-updates-and-deletes-with-linq-to-sql/
// Copyright(c) 2015 Terry Aney, MIT License https://bitbucket.org/terryaney/linqtosqlextensions/

namespace Carrotware.CMS.Data {

	public static class LinqHelper {
		private static string TableAlias = "sq0";
		private static string TableJoinAlias = "sq1";

		public static int BatchDelete<T>(this Table<T> table, IQueryable<T> entities) where T : class {
			DbCommand cmd = table.GetBatchDeleteDbCommand<T>(entities);

			IEnumerable<Object> parameters = (from p in cmd.Parameters.Cast<DbParameter>()
											  select p.Value);

			return table.Context.ExecuteCommand(cmd.CommandText, parameters.ToArray());
		}

		public static int BatchUpdate<T>(this Table<T> table, IQueryable<T> entities, Expression<Func<T, T>> evaluator) where T : class {
			DbCommand cmd = table.GetBatchUpdateDbCommand<T>(entities, evaluator);

			IEnumerable<Object> parameters = (from p in cmd.Parameters.Cast<DbParameter>()
											  select p.Value);

			return table.Context.ExecuteCommand(cmd.CommandText, parameters.ToArray());
		}

		internal static DbCommand GetBatchDeleteDbCommand<T>(this Table<T> table, IQueryable<T> entities) where T : class {
			DbCommand deleteCommand = table.Context.GetCommand(entities);

			deleteCommand.CommandText = String.Format("DELETE {0}\r\n{1}", TableAlias, table.GetJoinQuery<T>(entities));

			return deleteCommand;
		}

		internal static DbCommand GetBatchUpdateDbCommand<T>(this Table<T> table, IQueryable<T> entities, Expression<Func<T, T>> evaluator) where T : class {
			DbCommand updateCommand = table.Context.GetCommand(entities);

			string setParmVals = table.GetMemberInitExpression(entities, evaluator, updateCommand).ToString();

			updateCommand.CommandText = String.Format("UPDATE {0}\r\n{1}\r\n{2}", TableAlias, setParmVals, table.GetJoinQuery<T>(entities));

			if (updateCommand.CommandText.IndexOf("[arg0]") >= 0 || updateCommand.CommandText.IndexOf("NULL AS [EMPTY]") >= 0) {
				throw new NotSupportedException(String.Format("The evaluator Expression<Func<{0},{0}>> has processing that cannot be used during batch updating.", table.GetType()));
			}

			return updateCommand;
		}

		private static string GetJoinQuery<T>(this Table<T> table, IQueryable<T> entities) where T : class {
			MetaTable metaTable = table.Context.Mapping.GetTable(typeof(T));

			List<string> priKeys = (from mdm in metaTable.RowType.DataMembers
									where mdm.IsPrimaryKey
									select mdm.MappedName).ToList();

			var sbJoinOn = new StringBuilder();
			var sbSubQrySel = new StringBuilder();

			if (priKeys.Any()) {
				foreach (var key in priKeys) {
					if (sbJoinOn.Length > 0) {
						sbSubQrySel.AppendFormat(", [[tbl1]].[{0}]", key);
						sbJoinOn.AppendFormat(" AND [[tbl2]].[{0}] = [[tbl3]].[{0}]", key);
					} else {
						sbJoinOn.AppendFormat("[[tbl2]].[{0}] = [[tbl3]].[{0}]", key);
						sbSubQrySel.AppendFormat("[[tbl1]].[{0}]", key);
					}
				}

				sbJoinOn.Append(" ");
				sbSubQrySel.Append(" ");
			} else {
				throw new MissingPrimaryKeyException(String.Format("{0} does not have a PK.", metaTable.TableName));
			}

			DbCommand selectCommand = table.Context.GetCommand(entities);
			string selectText = selectCommand.CommandText;

			int endSelect = selectText.IndexOf("[");
			string selectStmt = selectText.Substring(0, endSelect);
			endSelect++;
			string tableAlias = selectText.Substring(endSelect, selectText.IndexOf("]", endSelect) - endSelect);
			string newTblAlias = String.Format("[{0}]", tableAlias);

			string joinOn = sbJoinOn.ToString().Replace("[[tbl2]]", TableAlias).Replace("[[tbl3]]", TableJoinAlias).Replace("[[tbl1]]", newTblAlias);
			string subQrySel = sbSubQrySel.ToString().Replace("[[tbl2]]", TableAlias).Replace("[[tbl3]]", TableJoinAlias).Replace("[[tbl1]]", newTblAlias);

			bool requiresTopClause = selectStmt.IndexOf(" TOP ") < 0 && selectText.IndexOf("\r\nORDER BY ") > 0;

			string subSelect = selectStmt
								+ (requiresTopClause ? " TOP 100 PERCENT " : String.Empty)
								+ subQrySel;

			subSelect += selectText.Substring(selectText.IndexOf("\r\nFROM "));

			var batchJoin = String.Format("FROM {0} AS {1} \r\nINNER JOIN ( {2} ) AS {3} ON ({4})\r\n", table.GetTableName(), TableAlias, subSelect, TableJoinAlias, joinOn);

			return batchJoin;
		}

		private static void TestExpression(ITable table, Expression expression) {
			PropertyInfo propertyInfo = table.Context.GetType().GetProperty("Provider", BindingFlags.Instance | BindingFlags.NonPublic);
			Object val = propertyInfo.GetValue(table.Context, null);
			MethodInfo methodInfo = val.GetType().GetMethod("System.Data.Linq.Provider.IProvider.Compile", BindingFlags.Instance | BindingFlags.NonPublic);

			methodInfo.Invoke(val, new object[] { expression });
		}

		private static string GetSqlSetValueStatement(ITable table, MethodCallExpression selectExpression, DbCommand updateCommand, string bindingName) {
			TestExpression(table, selectExpression);

			IQueryable selectQuery = (table as IQueryable).Provider.CreateQuery(selectExpression);

			string newBindName = String.Format("@p{0}", bindingName);

			DbCommand selectCmd = table.Context.GetCommand(selectQuery);
			string selectText = selectCmd.CommandText;

			int firstBracket = selectText.IndexOf("[") + 1;
			int fromIdx = selectText.IndexOf("\r\nFROM ");

			string tableAlias = selectText.Substring(firstBracket, selectText.IndexOf("]", firstBracket) - firstBracket);

			string newTblPrefix = String.Format("[{0}].", tableAlias);
			selectText = selectText.Substring(firstBracket - 1, fromIdx - firstBracket + 1);

			selectText = selectText.Replace(newTblPrefix, String.Empty)
									.Replace(" AS [value]", String.Empty)
									.Replace("@p", newBindName);

			foreach (DbParameter selectParam in selectCmd.Parameters.Cast<DbParameter>()) {
				string paramName = String.Format("@p{0}", updateCommand.Parameters.Count);

				selectText = selectText.Replace(selectParam.ParameterName.Replace("@p", newBindName), paramName);

				updateCommand.Parameters.Add(new SqlParameter(paramName, selectParam.Value));
			}

			if (!String.IsNullOrEmpty(selectText)) {
				if (!selectText.StartsWith("@") || selectText.StartsWith("[")) {
					selectText = String.Format("[{0}].{1}", TableAlias, selectText);
				}
			} else {
				selectText = String.Empty;
			}

			return selectText;
		}

		private static string GetTableName<T>(this Table<T> table) where T : class {
			MetaTable metaTable = table.Context.Mapping.GetTable(typeof(T));
			string tableName = metaTable.TableName;

			if (!tableName.Contains("[")) {
				string[] frags = tableName.Split('.');
				tableName = String.Format("[{0}]", String.Join("].[", frags));
			}

			return tableName;
		}

		private static string GetSqlSetStatement<T>(MemberInitExpression memberInitExpression, Table<T> table, DbCommand updateCommand) where T : class {
			Type entityType = typeof(T);

			if (memberInitExpression.Type != entityType) {
				throw new NotImplementedException(String.Format("The MemberInitExpression is initializing a class of the incorrect type '{0}' and it should be '{1}'.", memberInitExpression.Type, entityType));
			}

			string tableName = table.GetTableName();
			MetaTable metaTable = table.Context.Mapping.GetTable(entityType);
			List<MetaDataMember> metaMembers = (from mdm in metaTable.RowType.DataMembers select mdm).ToList();

			var sbSetParmValues = new StringBuilder();

			foreach (MemberBinding binding in memberInitExpression.Bindings) {
				MemberAssignment assignment = binding as MemberAssignment;

				if (assignment == null) {
					throw new NotImplementedException("All bindings inside the MemberInitExpression are expected to be of type MemberAssignment.");
				}

				string name = binding.Member.Name;

				if (sbSetParmValues.Length > 1) {
					sbSetParmValues.Append(", \r\n");
				}

				ParameterExpression entityParam = table.GetParameterExpression(updateCommand, assignment);

				MetaDataMember mdMember = (from c in metaMembers
										   where c.Name == name
										   select c).FirstOrDefault();

				if (mdMember == null) {
					throw new ArgumentOutOfRangeException(name, String.Format("The field '{0}' on table '{1}' could not be found.", name, tableName));
				}

				if (entityParam == null) {
					Object constant = Expression.Lambda(assignment.Expression, null).Compile().DynamicInvoke();

					if (constant == null) {
						sbSetParmValues.AppendFormat("[{0}] = null", mdMember.MappedName);
					} else {
						sbSetParmValues.AppendFormat("[{0}] = @p{1}", mdMember.MappedName, updateCommand.Parameters.Count);
						updateCommand.Parameters.Add(new SqlParameter(String.Format("@p{0}", updateCommand.Parameters.Count), constant));
					}
				} else {
					MethodCallExpression selectExpression = Expression.Call(typeof(Queryable), "Select", new Type[] { entityType, assignment.Expression.Type },
												Expression.Constant(table), Expression.Lambda(assignment.Expression, entityParam));

					sbSetParmValues.AppendFormat("[{0}] = {1}", mdMember.MappedName, GetSqlSetValueStatement(table, selectExpression, updateCommand, name));
				}
			}

			return "SET " + sbSetParmValues.ToString();
		}

		public static StringBuilder GetMemberInitExpression<T>(this Table<T> table, IQueryable<T> entities, Expression<Func<T, T>> evaluator, DbCommand updateCommand) where T : class {
			var sb = new StringBuilder();

			evaluator.Visit<MemberInitExpression>(delegate(MemberInitExpression expression) {
				if (sb.Length > 1) {
					throw new NotImplementedException("Only one MemberInitExpression is allowed for the evaluator parameter.");
				}

				sb.Append(GetSqlSetStatement<T>(expression, table, updateCommand));

				return expression;
			});

			sb.Append(String.Empty);

			return sb;
		}

		public static ParameterExpression GetParameterExpression<T>(this Table<T> table, DbCommand updateCommand, MemberAssignment assignment) where T : class {
			Type entityType = typeof(T);

			ParameterExpression entityParam = null;

			assignment.Expression.Visit<ParameterExpression>(delegate(ParameterExpression p) { if (p.Type == entityType) entityParam = p; return p; });

			return entityParam;
		}

		public static Expression Visit<T>(
					this Expression exp,
					Func<T, Expression> visitor) where T : Expression {
			return ExpressionVisitor<T>.Visit(exp, visitor);
		}
	}

	// =============================

	public class ExpressionVisitor<T> : ExpressionVisitor where T : Expression {
		private Func<T, Expression> _visitor;

		public ExpressionVisitor(Func<T, Expression> visitor) {
			_visitor = visitor;
		}

		public static Expression Visit(
			Expression exp,
			Func<T, Expression> visitor) {
			return new ExpressionVisitor<T>(visitor).Visit(exp);
		}

		public static Expression<TDelegate> Visit<TDelegate>(
			Expression<TDelegate> exp,
			Func<T, Expression> visitor) {
			return (Expression<TDelegate>)new ExpressionVisitor<T>(visitor).Visit(exp);
		}

		public override Expression Visit(Expression exp) {
			if (exp is T && _visitor != null) exp = _visitor((T)exp);

			return base.Visit(exp);
		}
	}
}