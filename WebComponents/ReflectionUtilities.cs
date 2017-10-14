using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Compilation;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public static class ReflectionUtilities {

		public static BindingFlags PublicInstanceStatic {
			get {
				return BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
			}
		}

		public static Type GetTypeFromString(string classString) {
			Type typ = null;
			if (!String.IsNullOrEmpty(classString)) {
				typ = Type.GetType(classString);

				if (typ == null && classString.IndexOf(",") < 1) {
					typ = BuildManager.GetType(classString, true);
				}
			}

			return typ;
		}

		public static Object GetPropertyValue(Object obj, string property) {
			PropertyInfo propertyInfo = obj.GetType().GetProperty(property, PublicInstanceStatic);
			return propertyInfo.GetValue(obj, null);
		}

		public static Object GetPropertyValueFlat(Object obj, string property) {
			PropertyInfo[] propertyInfos = obj.GetType().GetProperties(PublicInstanceStatic);
			PropertyInfo propertyInfo = null;
			foreach (PropertyInfo info in propertyInfos) {
				if (info.Name == property) {
					propertyInfo = info;
					break;
				}
			}
			if (propertyInfo != null) {
				return propertyInfo.GetValue(obj, null);
			} else {
				return null;
			}
		}

		public static bool DoesPropertyExist(Object obj, string property) {
			PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
			return propertyInfo == null ? false : true;
		}

		public static bool DoesPropertyExist(Type type, string property) {
			PropertyInfo propertyInfo = type.GetProperty(property);
			return propertyInfo == null ? false : true;
		}

		public static List<string> GetPropertyStrings(Object obj) {
			List<string> props = (from i in GetProperties(obj)
								  orderby i.Name
								  select i.Name).ToList();
			return props;
		}

		public static List<PropertyInfo> GetProperties(Object obj) {
			PropertyInfo[] info = obj.GetType().GetProperties(PublicInstanceStatic);

			List<PropertyInfo> props = (from i in info.AsEnumerable()
										orderby i.Name
										select i).ToList();
			return props;
		}

		public static List<string> GetPropertyStrings(Type type) {
			List<string> props = (from i in GetProperties(type)
								  orderby i.Name
								  select i.Name).ToList();
			return props;
		}

		public static List<PropertyInfo> GetProperties(Type type) {
			PropertyInfo[] info = type.GetProperties(PublicInstanceStatic);

			List<PropertyInfo> props = (from i in info.AsEnumerable()
										orderby i.Name
										select i).ToList();
			return props;
		}

		public static string GetPropertyString(Type type, string PropertyName) {
			string prop = (from i in GetProperties(type)
						   where i.Name.ToLowerInvariant().Trim() == PropertyName.ToLowerInvariant().Trim()
						   orderby i.Name
						   select i.Name).FirstOrDefault();
			return prop;
		}

		public static PropertyInfo GetProperty(Type type, string PropertyName) {
			PropertyInfo prop = (from i in GetProperties(type)
								 where i.Name.ToLowerInvariant().Trim() == PropertyName.ToLowerInvariant().Trim()
								 orderby i.Name
								 select i).FirstOrDefault();
			return prop;
		}

		public static string GetDescriptionAttribute(Type type, string fieldName) {
			PropertyInfo property = GetProperty(type, fieldName);
			if (property != null) {
				foreach (Attribute attr in property.GetCustomAttributes(typeof(DescriptionAttribute), true)) {
					if (attr != null) {
						DescriptionAttribute description = (DescriptionAttribute)attr;
						return description.Description;
					}
				}
			}

			return String.Empty;
		}

		public static PropertyInfo PropInfoFromExpression<T>(this T source, Expression<Func<T, Object>> expression) {
			string propertyName = String.Empty;
			PropertyInfo propInfo = null;

			MemberExpression memberExpression = expression.Body as MemberExpression ??
												((UnaryExpression)expression.Body).Operand as MemberExpression;
			if (memberExpression != null) {
				propertyName = memberExpression.Member.Name;

				switch (memberExpression.Expression.NodeType) {
					case ExpressionType.MemberAccess:
						string propName = ExtBuildProp(memberExpression);
						propInfo = typeof(T).GetProperty(propName);
						break;

					default:
						propInfo = typeof(T).GetProperty(propertyName);
						break;
				}
			}

			return propInfo;
		}

		public static Object GetPropValueFromExpression<T>(this T item, Expression<Func<T, Object>> property) {
			string columnName = ReflectionUtilities.BuildProp(property);
			PropertyInfo propInfo = item.PropInfoFromExpression<T>(property);
			Object val = propInfo.GetValue(item, null);
			Object obj = null;

			if (columnName.Contains(".")) {
				columnName = columnName.Substring(columnName.IndexOf(".") + 1);

				foreach (string colName in columnName.Split('.')) {
					obj = GetPropertyValue(val, colName);
					val = obj;
				}
			} else {
				obj = val;
			}

			return obj;
		}

		public static Object GetPropValueFromColumnName<T>(this T item, string columnName) {
			PropertyInfo propInfo = null;
			Object obj = null;
			Object val = null;

			if (columnName.Contains(".")) {
				foreach (string colName in columnName.Split('.')) {
					if (val == null) {
						obj = GetPropertyValue(item, colName);
					} else {
						obj = GetPropertyValue(val, colName);
					}
					val = obj;
				}
			} else {
				propInfo = GetProperty(typeof(T), columnName);
				val = propInfo.GetValue(item, null);
				obj = val;
			}

			return obj;
		}

		public static string BuildProp<T>(Expression<Func<T, Object>> property) {
			MemberExpression memberExpression = property.Body as MemberExpression ??
											((UnaryExpression)property.Body).Operand as MemberExpression;

			Expression expression = property.Body;
			string propertyName = String.Empty;

			if (memberExpression.NodeType == ExpressionType.MemberAccess) {
				expression = memberExpression;
			}

			while (expression.NodeType == ExpressionType.MemberAccess) {
				memberExpression = ((MemberExpression)expression);
				expression = memberExpression.Expression;

				if (String.IsNullOrEmpty(propertyName)) {
					propertyName = memberExpression.Member.Name;
				} else {
					propertyName = String.Format("{0}.{1}", memberExpression.Member.Name, propertyName);
				}
			}

			if (expression.NodeType != ExpressionType.MemberAccess) {
				if (String.IsNullOrEmpty(propertyName)) {
					propertyName = memberExpression.Member.Name;
				}
			}

			return propertyName;
		}

		public static string ExtBuildProp(Expression expression) {
			MemberExpression memberExpression = expression as MemberExpression ??
												((UnaryExpression)expression).Operand as MemberExpression;

			if (memberExpression.NodeType == ExpressionType.MemberAccess) {
				expression = memberExpression;
			}

			string propertyName = String.Empty;

			while (expression.NodeType == ExpressionType.MemberAccess) {
				memberExpression = ((MemberExpression)expression);
				expression = memberExpression.Expression;
			}

			if (expression.NodeType != ExpressionType.MemberAccess) {
				if (String.IsNullOrEmpty(propertyName)) {
					propertyName = memberExpression.Member.Name;
				}
			}

			return propertyName;
		}

		private static Expression ParseExpression(Expression expression) {
			while (expression.NodeType == ExpressionType.MemberAccess) {
				expression = ((MemberExpression)expression).Expression;
			}

			if (expression.NodeType != ExpressionType.MemberAccess) {
				return (ParameterExpression)expression;
			}

			return null;
		}

		public static Object GetAttribute<T>(Type type, string memberName) {
			MemberInfo[] memInfo = type.GetMember(memberName);

			if (memInfo != null && memInfo.Length > 0) {
				foreach (var m in memInfo) {
					object[] attrs = m.GetCustomAttributes(typeof(T), false);

					if (attrs != null && attrs.Length > 0) {
						return ((T)attrs[0]);
					}
				}
			}

			return null;
		}

		public static IQueryable<T> SortByParm<T>(this IList<T> source, string sortByFieldName, string sortDirection) {
			return SortByParm<T>(source.AsQueryable(), sortByFieldName, sortDirection);
		}

		public static IQueryable<T> SortByParm<T>(this IQueryable<T> source, string sortByFieldName, string sortDirection) {
			sortDirection = String.IsNullOrEmpty(sortDirection) ? "ASC" : sortDirection.Trim().ToUpperInvariant();

			string SortDir = sortDirection.Contains("DESC") ? "OrderByDescending" : "OrderBy";

			Type type = typeof(T);
			ParameterExpression parameter = Expression.Parameter(type, "source");

			PropertyInfo property = null;
			Expression propertyAccess = null;

			if (sortByFieldName.Contains('.')) {
				//handles complex child properties
				string[] childProps = sortByFieldName.Split('.');
				property = type.GetProperty(childProps[0]);
				propertyAccess = Expression.MakeMemberAccess(parameter, property);
				for (int i = 1; i < childProps.Length; i++) {
					property = property.PropertyType.GetProperty(childProps[i]);
					propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
				}
			} else {
				property = type.GetProperty(sortByFieldName);
				propertyAccess = Expression.MakeMemberAccess(parameter, property);
			}

			LambdaExpression orderByExp = Expression.Lambda(propertyAccess, parameter);

			MethodCallExpression resultExp = Expression.Call(typeof(Queryable), SortDir, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));

			return source.Provider.CreateQuery<T>(resultExp);
		}
	}
}