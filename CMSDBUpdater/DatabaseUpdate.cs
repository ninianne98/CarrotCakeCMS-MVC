using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

namespace Carrotware.CMS.DBUpdater {

	public class DatabaseUpdate {

		public DatabaseUpdate() { }

		public DatabaseUpdate(bool clearTest) {
			if (clearTest) {
				DatabaseSchemaState.LastSQLError = null;
				DatabaseSchemaState.ResetSQLState();
				TestDatabaseWithQuery();
			}
		}

		private void TestDatabaseWithQuery() {
			DatabaseSchemaState.LastSQLError = null;

			string query = "select top 10 table_name, column_name, ordinal_position from [INFORMATION_SCHEMA].[COLUMNS] as isc \n" +
					" where isc.table_name like 'carrot%' \n" +
					" order by isc.table_name, isc.ordinal_position, isc.column_name";

			DataTable table1 = DatabaseSchemaState.GetTestData(query);
		}

		public List<DatabaseUpdateResponse> CreateCMSDatabase() {
			var res = new List<DatabaseUpdateResponse>();

			if (!DatabaseSchemaState.FailedSQL) {
				bool bDbResult = SQLUpdateNugget.EvalNuggetKey("DoCMSTablesExist");
				bool bAuthResult = SQLUpdateNugget.EvalNuggetKey("DoAuthTablesExist");

				var res1 = new DatabaseUpdateResponse();

				if (!bAuthResult) {
					res1.LastException = ExecFileContents("MEMBER01.sql", false);
					res1.Response = "Created Membership";
					res1.RanUpdate = true;
				} else {
					res1.Response = "Membership Already Created";
				}
				res.Add(res1);

				var res2 = new DatabaseUpdateResponse();

				if (!bDbResult) {
					res2.LastException = ExecFileContents("CREATE01.sql", false);
					res2.Response = "Created Database";
					res2.RanUpdate = true;
					// change version key when the DB creation is re-scripted
					DatabaseSchemaState.SetDbSchemaVersion(DatabaseSchemaState.DbVersion02);
				} else {
					res2.Response = "Database Already Created";
				}
				res.Add(res2);

				return res;
			}

			var res3 = new DatabaseUpdateResponse();
			res3.Response = "*** Database Access Failed ***";
			res3.LastException = new ApplicationException(res3.Response);
			res.Add(res3);

			return res;
		}

		public bool DoCMSTablesExist() {
			if (!DatabaseSchemaState.FailedSQL) {
				bool bTestResult = SQLUpdateNugget.EvalNuggetKey("DoCMSTablesExist");

				if (bTestResult) {
					return bTestResult;
				}
			}

			return false;
		}

		public List<DatabaseUpdateMessage> MergeMessages(List<DatabaseUpdateMessage> lstMsgs1, List<DatabaseUpdateMessage> lstMsgs2) {
			if (lstMsgs1 == null) {
				lstMsgs1 = new List<DatabaseUpdateMessage>();
			}

			if (lstMsgs2 == null) {
				lstMsgs2 = new List<DatabaseUpdateMessage>();
			}

			if (lstMsgs2.Any()) {
				int iPad = lstMsgs1.Count;
				lstMsgs2.ToList().ForEach(x => x.Order = (x.Order + iPad));

				lstMsgs1 = lstMsgs1.Union(lstMsgs2).ToList();
			}

			return lstMsgs1;
		}

		public List<DatabaseUpdateMessage> HandleResponse(List<DatabaseUpdateMessage> lstMsgs, Exception ex) {
			if (lstMsgs == null) {
				lstMsgs = new List<DatabaseUpdateMessage>();
			}

			var execMessage = new DatabaseUpdateResponse();
			execMessage.LastException = ex;
			execMessage.Response = "An error occurred.";

			HandleResponse(lstMsgs, "Error: ", execMessage);

			return lstMsgs;
		}

		public List<DatabaseUpdateMessage> HandleResponse(List<DatabaseUpdateMessage> lstMsgs, string sMsg) {
			if (lstMsgs == null) {
				lstMsgs = new List<DatabaseUpdateMessage>();
			}

			var execMessage = new DatabaseUpdateResponse();

			HandleResponse(lstMsgs, sMsg, execMessage);

			return lstMsgs;
		}

		public List<DatabaseUpdateMessage> HandleResponse(List<DatabaseUpdateMessage> lstMsgs, string sMsg, List<DatabaseUpdateResponse> lstExecMessages) {
			if (lstMsgs == null) {
				lstMsgs = new List<DatabaseUpdateMessage>();
			}

			int m = 1;
			if (lstExecMessages != null) {
				foreach (var msg in lstExecMessages) {
					lstMsgs = HandleResponse(lstMsgs, string.Format("{0}  [{1}]", sMsg, m), msg);
					m++;
				}
			}

			return lstMsgs;
		}

		public List<DatabaseUpdateMessage> HandleResponse(List<DatabaseUpdateMessage> lstMsgs, string sMsg, DatabaseUpdateResponse execMessage) {
			if (lstMsgs == null) {
				lstMsgs = new List<DatabaseUpdateMessage>();
			}

			var item = new DatabaseUpdateMessage();

			if (!string.IsNullOrEmpty(sMsg)) {
				item.Message = sMsg;

				if (execMessage != null && (execMessage.Response.Length > 0 || execMessage.LastException != null)) {
					item.AlteredData = execMessage.RanUpdate;
					item.Response = execMessage.Response;

					if (execMessage.LastException != null && !string.IsNullOrEmpty(execMessage.LastException.Message)) {
						DatabaseSchemaState.WriteDebugException("handleresponse", execMessage.LastException);

						item.HasException = true;
						item.ExceptionText = execMessage.LastException.Message;
						if (execMessage.LastException.InnerException != null && !string.IsNullOrEmpty(execMessage.LastException.InnerException.Message)) {
							item.InnerExceptionText = execMessage.LastException.InnerException.Message;
						}
					}
				}
			}

			item.Order = lstMsgs.Count + 1;

			lstMsgs.Add(item);

			return lstMsgs;
		}

		public string BuildUpdateString(int iCount) {
			return "Update " + (iCount).ToString() + " ";
		}

		private static object updateLocker = new object();

		public DatabaseUpdateStatus PerformUpdates() {
			DatabaseUpdateStatus status = new DatabaseUpdateStatus();
			bool bUpdate = true;
			var lst = new List<DatabaseUpdateMessage>();

			lock (updateLocker) {
				if (!DoCMSTablesExist()) {
					HandleResponse(lst, "Create Database", CreateCMSDatabase());
				} else {
					HandleResponse(lst, "Database already exists");
				}

				var ver = DatabaseSchemaState.GetDbSchemaVersion();

				bUpdate = DatabaseNeedsUpdate()
						&& ver.DataValue != DatabaseSchemaState.CurrentDbVersion;

				int iUpdate = 1;

				if (bUpdate) {
					if (ver.DataValue != DatabaseSchemaState.CurrentDbVersion) {
						ver = DatabaseSchemaState.GetDbSchemaVersion();

						var oldupdates = new string[] { "2015", "2016", "2017", "2018", "2019" };

						if (ver.DataValue != DatabaseSchemaState.CurrentDbVersion) {
							ver = DatabaseSchemaState.GetDbSchemaVersion();

							if (oldupdates.Where(x => ver.IsYearOf(x)).Any()) {
								HandleResponse(lst, BuildUpdateString(iUpdate++), AlterStep01());
								HandleResponse(lst, BuildUpdateString(iUpdate++), AlterStep02());
							}
						}
					}
				}

				ver = DatabaseSchemaState.GetDbSchemaVersion();

				if (ver.DataValue != DatabaseSchemaState.CurrentDbVersion) {
					HandleResponse(lst, BuildUpdateString(iUpdate++), Refresh01());
					HandleResponse(lst, "Database up-to-date [" + ver.DataValue + "] ");
				}

				DatabaseSchemaState.ResetFailedSQL();

				DatabaseSchemaState.ResetSQLState();

				bUpdate = DatabaseNeedsUpdate();

				status.NeedsUpdate = bUpdate;
				status.Messages = lst;
			}

			return status;
		}

		public bool TableExists(string testTableName) {
			string testQuery = "select * from [INFORMATION_SCHEMA].[COLUMNS] where table_name = @TableName ";
			List<SqlParameter> parms = new List<SqlParameter>();

			SqlParameter parmKey = new SqlParameter();
			parmKey.ParameterName = "@TableName";
			parmKey.SqlDbType = SqlDbType.VarChar;
			parmKey.Size = 2000;
			parmKey.Direction = ParameterDirection.Input;
			parmKey.Value = testTableName;

			parms.Add(parmKey);

			DataTable table1 = DatabaseSchemaState.GetTestData(testQuery, parms);

			if (table1.Rows.Count < 1) {
				return false;
			}

			return true;
		}

		public List<string> GetTableColumns(string testTableName) {
			List<string> lst = new List<string>();

			string testQuery = "select * from [INFORMATION_SCHEMA].[COLUMNS] where table_name = @TableName ";

			List<SqlParameter> parms = new List<SqlParameter>();

			SqlParameter parmKey = new SqlParameter();
			parmKey.ParameterName = "@TableName";
			parmKey.SqlDbType = SqlDbType.VarChar;
			parmKey.Size = 2000;
			parmKey.Direction = ParameterDirection.Input;
			parmKey.Value = testTableName;

			parms.Add(parmKey);

			DataTable table1 = DatabaseSchemaState.GetTestData(testQuery, parms);

			if (table1.Rows.Count > 1) {
				lst = (from d in table1.AsEnumerable()
					   select d.Field<string>("column_name")).ToList();
			}

			return lst;
		}

		public DatabaseUpdateResponse ApplyUpdateIfNotFound(string testQuery, string updateStatement, bool bIgnore) {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();
			DataTable table1 = DatabaseSchemaState.GetTestData(testQuery);

			if (table1.Rows.Count < 1) {
				res.LastException = ExecScriptContents(updateStatement, bIgnore);
				res.Response = "Applied update";
				res.RanUpdate = true;
				return res;
			}

			res.Response = "Did not apply any updates";
			return res;
		}

		public DatabaseUpdateResponse ApplyUpdateIfFound(string testQuery, string updateStatement, bool bIgnore) {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();
			DataTable table1 = DatabaseSchemaState.GetTestData(testQuery);

			if (table1.Rows.Count > 0) {
				res.LastException = ExecScriptContents(updateStatement, bIgnore);
				res.Response = "Applied update";
				res.RanUpdate = true;
				return res;
			}

			res.Response = "Did not apply any updates";
			return res;
		}

		public bool DatabaseNeedsUpdate() {
			if (!DatabaseSchemaState.FailedSQL) {
				bool bTestResult = false;

				var ver = DatabaseSchemaState.GetDbSchemaVersion();

				bTestResult = ver.DataValue != DatabaseSchemaState.CurrentDbVersion;
				if (bTestResult) {
					return true;
				}

				bTestResult = SQLUpdateNugget.EvalNuggetKey("DatabaseNeedsUpdate");
				if (bTestResult) {
					return true;
				}

				bTestResult = SQLUpdateNugget.EvalManditoryChecks();
				if (bTestResult) {
					return true;
				}
			}

			return false;
		}

		public DatabaseUpdateResponse AlterStep01() {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();

			bool bTestResult = SQLUpdateNugget.EvalNuggetKey("AlterStep01");

			if (bTestResult) {
				res.LastException = ExecFileContents("ALTER01.sql", false);
				res.Response = "Update comment view";
				res.RanUpdate = true;
				DatabaseSchemaState.SetDbSchemaVersion(DatabaseSchemaState.DbVersion01);
				return res;
			} else {
				// if the db version is off, check leading tidbit against current and immediate prior
				var ver = DatabaseSchemaState.GetDbSchemaVersion();

				if (ver.IsMinorOf(DatabaseSchemaState.DbVersion00)
							|| ver.IsMinorOf(DatabaseSchemaState.DbVersion01)
							|| ver.IsYearOf("2015") || ver.IsYearOf("2016")) {
					DatabaseSchemaState.SetDbSchemaVersion(DatabaseSchemaState.DbVersion01);
				}
			}

			res.Response = "Comment view update already applied";
			return res;
		}

		public DatabaseUpdateResponse AlterStep02() {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();

			bool bTestResult = SQLUpdateNugget.EvalNuggetKey("AlterStep02");

			if (bTestResult) {
				res.LastException = ExecFileContents("ALTER02.sql", false);
				res.Response = "Update timezone sproc";
				res.RanUpdate = true;
				DatabaseSchemaState.SetDbSchemaVersion(DatabaseSchemaState.DbVersion02);
				return res;
			} else {
				// if the db version is off, check leading tidbit against current and immediate prior
				var ver = DatabaseSchemaState.GetDbSchemaVersion();

				if (ver.IsMinorOf(DatabaseSchemaState.DbVersion01)
						|| ver.IsMinorOf(DatabaseSchemaState.DbVersion02)) {
					DatabaseSchemaState.SetDbSchemaVersion(DatabaseSchemaState.DbVersion02);
				}
			}

			res.Response = "Timezone sproc update already applied";
			return res;
		}

		public DatabaseUpdateResponse Refresh01() {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();

			var ver = DatabaseSchemaState.GetDbSchemaVersion();
			var priorVer = ver.IsMinorOf(DatabaseSchemaState.DbVersion00) || ver.IsMinorOf(DatabaseSchemaState.DbVersion01);
			var minorUpdate = ver.IsMinorOf(DatabaseSchemaState.DbVersion02);

			if (priorVer || minorUpdate == false) {
				res.LastException = ExecFileContents("REFRESH01.sql", false);
				res.Response = "Refreshed views and sprocs";
				res.RanUpdate = true;

				DatabaseSchemaState.SetDbSchemaVersion(DatabaseSchemaState.DbVersion02);
			}

			res.Response = "Refresh of views and sprocs already applied";
			return res;
		}

		private List<string> SplitScriptAtGo(string sqlQuery) {
			sqlQuery += "\r\n\r\nGO\r\n\r\n";
			sqlQuery = sqlQuery.Replace("\r\n", "\n");

			string[] splitcommands = sqlQuery.Split(new string[] { "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
			List<string> commandList = new List<string>(splitcommands);
			return commandList;
		}

		public Exception ExecScriptContents(string scriptContents, bool bIgnoreErr) {
			string _connStr = DatabaseSchemaState.SetConn();

			return ExecScriptContents(_connStr, scriptContents, bIgnoreErr);
		}

		public Exception ExecScriptContents(string connectionString, string scriptContents, bool bIgnoreErr) {
			return ExecNonQuery(connectionString, scriptContents, bIgnoreErr);
		}

		private Exception ExecFileContents(string resouceName, bool bIgnoreErr) {
			string _connStr = DatabaseSchemaState.SetConn();

			return ExecFileContents(_connStr, resouceName, bIgnoreErr);
		}

		private Exception ExecFileContents(string connectionString, string resourceName, bool ignoreErr) {
			string scriptContents = DatabaseSchemaState.ReadEmbededScript(resourceName);

			Exception response = ExecScriptContents(connectionString, scriptContents, ignoreErr);

			return response;
		}

		private Exception ExecNonQuery(string connectionString, string sqlQuery, bool bIgnoreErr) {
			Exception exc = new Exception("");

			using (SqlConnection cn = new SqlConnection(connectionString)) {
				cn.Open();

				List<string> cmdLst = SplitScriptAtGo(sqlQuery);

				if (!bIgnoreErr) {
					try {
						foreach (string cmdStr in cmdLst) {
							using (SqlCommand cmd = cn.CreateCommand()) {
								cmd.CommandText = cmdStr;
								cmd.Connection = cn;
								cmd.CommandTimeout = 360;
								int ret = cmd.ExecuteNonQuery();
							}
						}
					} catch (Exception ex) {
						exc = ex;
						DatabaseSchemaState.WriteDebugException("execnonquery-ignore", ex);
					} finally {
						cn.Close();
					}
				} else {
					var sb = new StringBuilder();
					foreach (string cmdStr in cmdLst) {
						try {
							using (SqlCommand cmd = cn.CreateCommand()) {
								cmd.CommandText = cmdStr;
								cmd.Connection = cn;
								cmd.CommandTimeout = 360;
								int ret = cmd.ExecuteNonQuery();
							}
						} catch (Exception ex) {
							sb.Append(ex.Message + "\n" + ex.StackTrace + "\n~~~~~~~~~~~~~~~~~~~~~~~~\n");
							if (ex.InnerException != null) {
								sb.Append(ex.InnerException.Message + "\n" + ex.InnerException.StackTrace + "\n~~~~~~~~~~~~~~~~~~~~~~~~\n");
							}
							DatabaseSchemaState.WriteDebugException("execnonquery", ex);
						}
					}
					exc = new Exception(sb.ToString());
					cn.Close();
				}
			}

			return exc;
		}
	}

	//======================
	public class DatabaseUpdateStatus {
		public bool NeedsUpdate { get; set; }

		public List<DatabaseUpdateMessage> Messages { get; set; }

		public DatabaseUpdateStatus() {
			this.Messages = new List<DatabaseUpdateMessage>();
			this.NeedsUpdate = true;
		}
	}

	//======================
	public class DataInfo {
		public string DataKey { get; set; } = "Key";
		public string DataValue { get; set; } = "00000000";

		public bool IsYearOf(string testVersion) {
			return IsSubVersionOf(testVersion, 4);
		}

		public bool IsMinorOf(string testVersion) {
			return IsSubVersionOf(testVersion, 6);
		}

		protected bool IsSubVersionOf(string testVersion, int len) {
			if (string.IsNullOrEmpty(this.DataValue) || string.IsNullOrEmpty(testVersion)) {
				return false;
			}
			if (this.DataValue.Length < len || testVersion.Length < len) {
				return false;
			}

			return this.DataValue.Substring(0, len) == testVersion.Substring(0, len);
		}

		public static string DBSchema {
			get { return "DBSchema"; }
		}

		public static DataInfo CreateBlankSchema() {
			var di = new DataInfo();
			di.DataKey = DataInfo.DBSchema;
			di.DataValue = "000000";
			return di;
		}
	}

	//======================
	public class DatabaseUpdateMessage {
		public string Message { get; set; } = string.Empty;
		public string ExceptionText { get; set; }
		public string InnerExceptionText { get; set; }
		public string Response { get; set; } = string.Empty;
		public int Order { get; set; } = -1;
		public bool AlteredData { get; set; }
		public bool HasException { get; set; }

		public DatabaseUpdateMessage() {
			this.ExceptionText = null;
			this.InnerExceptionText = null;
			this.AlteredData = false;
			this.HasException = false;
			this.Message = string.Empty;
			this.Response = string.Empty;
			this.Order = -1;
		}
	}

	//======================
	public class DatabaseUpdateResponse {
		public Exception LastException { get; set; }
		public string Response { get; set; } = string.Empty;
		public bool RanUpdate { get; set; }

		public DatabaseUpdateResponse() {
			this.LastException = null;
			this.Response = string.Empty;
			this.RanUpdate = false;
		}

		public void Combine(DatabaseUpdateResponse res1, DatabaseUpdateResponse res2) {
			if (res1.LastException != null && res2.LastException != null) {
				var msg1 = res1.LastException.CombineMessage();
				var msg2 = res2.LastException.CombineMessage();

				this.LastException = new Exception(msg1, new Exception(msg2));
			} else {
				this.LastException = (res1.LastException != null) ? res1.LastException : res2.LastException;
			}

			this.Response = string.Join("; ", new string[] { res1.Response, res2.Response });
			this.RanUpdate = res1.RanUpdate || res2.RanUpdate;
		}
	}
}