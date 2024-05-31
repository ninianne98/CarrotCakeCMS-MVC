using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;

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
		public static SqlException LastSQLError { get; set; }

		public static string CurrentDbVersion { get { return DbVersion02; } }

		public static string DbVersion00 { get { return "20150829"; } }

		public static string DbVersion01 { get { return "20151001"; } }

		public static string DbVersion02 { get { return "20200915"; } }

		public DatabaseUpdate() { }

		public DatabaseUpdate(bool clearTest) {
			if (clearTest) {
				DatabaseUpdate.LastSQLError = null;
				DatabaseUpdate.ResetSQLState();
				TestDatabaseWithQuery();
			}
		}

		public static string ReadEmbededScript(string resouceName) {
			var sb = new StringBuilder();

			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = new StreamReader(assembly.GetManifestResourceStream(resouceName))) {
				sb.Append(stream.ReadToEnd());
			}

			return sb.ToString();
		}

		private void TestDatabaseWithQuery() {
			DatabaseUpdate.LastSQLError = null;

			string query = "select top 10 table_name, column_name, ordinal_position from [INFORMATION_SCHEMA].[COLUMNS] as isc " +
					" where isc.table_name like 'carrot%' " +
					" order by isc.table_name, isc.ordinal_position, isc.column_name";

			DataTable table1 = GetTestData(query);
		}

		private static string SetConn() {
			string _connStr = string.Empty;

			if (ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"] != null) {
				var conString = ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"];
				_connStr = conString.ConnectionString;
			}

			return _connStr;
		}

		private static string _contentKey = "cms_SiteSetUpSQLState";

		public static bool FailedSQL {
			get {
				bool c = false;
				var ret = GetCacheItem(_contentKey);
				try { c = Convert.ToBoolean(ret); } catch { }
				return c;
			}
			set {
				HttpContext.Current.Cache.Insert(_contentKey, value, null, DateTime.Now.AddMinutes(3), Cache.NoSlidingExpiration);
			}
		}

		public static void ResetFailedSQL() {
			HttpContext.Current.Cache.Insert(_contentKey, "False", null, DateTime.Now.AddMilliseconds(10), Cache.NoSlidingExpiration);
			HttpContext.Current.Cache.Remove(_contentKey);
		}

		public static bool SystemNeedsChecking(Exception ex) {
			//assumption is database is probably empty / needs updating, so trigger the under construction view

			if (ex is SqlException && ex != null) {
				string msg = ex.Message.ToLower();
				if (ex.InnerException != null) {
					msg += "\r\n" + ex.InnerException.Message.ToLower();
				}
				if (msg.Contains("the server was not found")) {
					return false;
				}

				if (msg.Contains("invalid object name")
					//|| msg.Contains("no process is on the other end of the pipe")
					|| msg.Contains("invalid column name")
					|| msg.Contains("could not find stored procedure")
					|| msg.Contains("not found")) {
					return true;
				}
			}

			return false;
		}

		public static object GetCacheItem(string key) {
			if (HttpContext.Current.Cache[key] != null) {
				return HttpContext.Current.Cache[key];
			}
			return null;
		}

		public static string GetCacheItemString(string key) {
			var item = GetCacheItem(key);
			return item != null ? item.ToString() : null;
		}

		public DatabaseUpdateResponse CreateCMSDatabase() {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();

			if (!DatabaseUpdate.FailedSQL) {
				bool bTestResult = SQLUpdateNugget.EvalNuggetKey("DoCMSTablesExist");

				if (!bTestResult) {
					res.LastException = ExecFileContents("Carrotware.CMS.DBUpdater.DataScripts.CREATE01.sql", false);
					res.Response = "Created Database";
					res.RanUpdate = true;
					// change version key when the DB creation is re-scripted
					SetDbSchemaVersion(DatabaseUpdate.DbVersion02);
					return res;
				}

				res.Response = "Database Already Created";
				return res;
			}

			res.Response = "*** Database Access Failed ***";
			return res;
		}

		public bool DoCMSTablesExist() {
			if (!DatabaseUpdate.FailedSQL) {
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

			DatabaseUpdateResponse execMessage = new DatabaseUpdateResponse();
			execMessage.LastException = ex;
			execMessage.Response = "An error occurred.";

			HandleResponse(lstMsgs, "Error: ", execMessage);

			return lstMsgs;
		}

		public List<DatabaseUpdateMessage> HandleResponse(List<DatabaseUpdateMessage> lstMsgs, string sMsg) {
			if (lstMsgs == null) {
				lstMsgs = new List<DatabaseUpdateMessage>();
			}

			HandleResponse(lstMsgs, sMsg, null);

			return lstMsgs;
		}

		public List<DatabaseUpdateMessage> HandleResponse(List<DatabaseUpdateMessage> lstMsgs, string sMsg, DatabaseUpdateResponse execMessage) {
			if (lstMsgs == null) {
				lstMsgs = new List<DatabaseUpdateMessage>();
			}

			DatabaseUpdateMessage item = new DatabaseUpdateMessage();

			if (!string.IsNullOrEmpty(sMsg)) {
				item.Message = sMsg;

				if (execMessage != null) {
					item.AlteredData = execMessage.RanUpdate;
					item.Response = execMessage.Response;

					if (execMessage.LastException != null && !string.IsNullOrEmpty(execMessage.LastException.Message)) {
						WriteDebugException("handleresponse", execMessage.LastException);

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

				bool isDbCurrent = false;

				bUpdate = DatabaseNeedsUpdate();

				DataInfo ver = GetDbSchemaVersion();

				int iUpdate = 1;

				if (bUpdate || (ver.DataValue != DatabaseUpdate.CurrentDbVersion)) {
					if (ver.DataValue != DatabaseUpdate.CurrentDbVersion) {
						ver = GetDbSchemaVersion();

						var oldupdates = new string[] { "2015", "2016", "2017", "2018", "2019" };

						if (ver.DataValue != DatabaseUpdate.CurrentDbVersion) {
							ver = GetDbSchemaVersion();

							if (oldupdates.Where(x => ver.DataValue.StartsWith(x)).Any()) {
								HandleResponse(lst, BuildUpdateString(iUpdate++), AlterStep01());
								HandleResponse(lst, BuildUpdateString(iUpdate++), AlterStep02());
							}
						}
					} else {
						isDbCurrent = true;
					}
				} else {
					isDbCurrent = true;
				}

				if (isDbCurrent) {
					HandleResponse(lst, BuildUpdateString(iUpdate++), Refresh01());
					HandleResponse(lst, "Database up-to-date [" + ver.DataValue + "] ");
				}

				ResetFailedSQL();

				ResetSQLState();

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

			DataTable table1 = GetTestData(testQuery, parms);

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

			DataTable table1 = GetTestData(testQuery, parms);

			if (table1.Rows.Count > 1) {
				lst = (from d in table1.AsEnumerable()
					   select d.Field<string>("column_name")).ToList();
			}

			return lst;
		}

		public DatabaseUpdateResponse ApplyUpdateIfNotFound(string testQuery, string updateStatement, bool bIgnore) {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();
			DataTable table1 = GetTestData(testQuery);

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
			DataTable table1 = GetTestData(testQuery);

			if (table1.Rows.Count > 0) {
				res.LastException = ExecScriptContents(updateStatement, bIgnore);
				res.Response = "Applied update";
				res.RanUpdate = true;
				return res;
			}

			res.Response = "Did not apply any updates";
			return res;
		}

		private static object logLocker = new object();

		public static void WriteDebugException(string debugSource, Exception objErr) {
			WriteDebugException(false, debugSource, objErr);
		}

		public static void WriteDebugException(bool bWriteError, string debugSource, Exception objErr) {
#if DEBUG
			bWriteError = true; // always write errors when debug build
#endif

			if (bWriteError && objErr != null) {
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("----------------  " + debugSource.ToUpperInvariant() + " - " + DateTime.Now.ToString() + "  ----------------");

				sb.AppendLine("[" + objErr.GetType().ToString() + "] " + objErr.Message);

				if (objErr.StackTrace != null) {
					sb.AppendLine(objErr.StackTrace);
				}

				if (objErr.InnerException != null) {
					sb.AppendLine(objErr.InnerException.Message);
				}

				string filePath = HttpContext.Current.Server.MapPath("~/carrot_errors.txt");

				Encoding encode = Encoding.Default;
				lock (logLocker) {
					using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)) {
						using (StreamWriter oWriter = new StreamWriter(fs, encode)) {
							oWriter.Write(sb.ToString());
						}
					}
				}
			}
		}

		private static string _contentSqlStateKey = "cms_SqlTablesIncomplete";

		public static bool TablesIncomplete {
			get {
				string tablesIncomplete = string.Empty;
				bool c = true;
				var ret = GetCacheItemString(_contentSqlStateKey);

				if (ret != null) {
					tablesIncomplete = ret;
				} else {
					try {
						c = AreCMSTablesIncomplete();
					} catch (Exception ex) {
						c = false;
						WriteDebugException("tablesincomplete", ex);
					}

					tablesIncomplete = c.ToString();
					HttpContext.Current.Cache.Insert(_contentSqlStateKey, tablesIncomplete, null, DateTime.Now.AddMinutes(3), Cache.NoSlidingExpiration);
				}

				c = Convert.ToBoolean(tablesIncomplete);
				return c;
			}
		}

		public static void ResetSQLState() {
			var ret = GetCacheItem(_contentSqlStateKey);
			if (ret != null) {
				HttpContext.Current.Cache.Remove(_contentSqlStateKey);
			}
		}

		public static bool AreCMSTablesIncomplete() {
			if (!DatabaseUpdate.FailedSQL) {
				bool bTestResult = false;

				DataInfo ver = GetDbSchemaVersion();
				bTestResult = ver.DataValue != DatabaseUpdate.CurrentDbVersion;
				if (bTestResult) {
					return true;
				}

				bTestResult = SQLUpdateNugget.EvalNuggetKey("AreCMSTablesIncomplete");
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

		public bool DatabaseNeedsUpdate() {
			if (!DatabaseUpdate.FailedSQL) {
				bool bTestResult = false;

				DataInfo ver = GetDbSchemaVersion();
				bTestResult = ver.DataValue != DatabaseUpdate.CurrentDbVersion;
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

		public static bool UsersExist {
			get {
				if (!DatabaseUpdate.FailedSQL) {
					try {
						bool bTestResult = SQLUpdateNugget.EvalNuggetKey("DoUsersExist");

						return bTestResult;
					} catch (Exception ex) {
						WriteDebugException("usersexist", ex);
					}
				}

				return false;
			}
		}

		public DatabaseUpdateResponse AlterStep01() {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();

			bool bTestResult = SQLUpdateNugget.EvalNuggetKey("AlterStep01");

			if (bTestResult) {
				res.LastException = ExecFileContents("Carrotware.CMS.DBUpdater.DataScripts.ALTER01.sql", false);
				res.Response = "Update comment view";
				res.RanUpdate = true;
				SetDbSchemaVersion(DatabaseUpdate.DbVersion01);
				return res;
			} else {
				// if the db version is off, check leading tidbit against current and immediate prior
				DataInfo ver = GetDbSchemaVersion();

				if (DatabaseUpdate.DbVersion00.Substring(0, 6) == ver.DataValue.Substring(0, 6)
					|| DatabaseUpdate.DbVersion01.Substring(0, 6) == ver.DataValue.Substring(0, 6)
					|| "2015" == ver.DataValue.Substring(0, 4) || "2016" == ver.DataValue.Substring(0, 4)) {
					SetDbSchemaVersion(DatabaseUpdate.DbVersion01);
				}
			}

			res.Response = "Comment view update already applied";
			return res;
		}

		public DatabaseUpdateResponse AlterStep02() {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();

			bool bTestResult = SQLUpdateNugget.EvalNuggetKey("AlterStep02");

			if (bTestResult) {
				res.LastException = ExecFileContents("Carrotware.CMS.DBUpdater.DataScripts.ALTER02.sql", false);
				res.Response = "Update timezone sproc";
				res.RanUpdate = true;
				SetDbSchemaVersion(DatabaseUpdate.DbVersion02);
				return res;
			} else {
				// if the db version is off, check leading tidbit against current and immediate prior
				DataInfo ver = GetDbSchemaVersion();

				if (DatabaseUpdate.DbVersion01.Substring(0, 6) == ver.DataValue.Substring(0, 6)
					|| DatabaseUpdate.DbVersion02.Substring(0, 6) == ver.DataValue.Substring(0, 6)) {
					SetDbSchemaVersion(DatabaseUpdate.DbVersion02);
				}
			}

			res.Response = "Timezone sproc update already applied";
			return res;
		}

		public DatabaseUpdateResponse Refresh01() {
			DatabaseUpdateResponse res = new DatabaseUpdateResponse();

			res.LastException = ExecFileContents("Carrotware.CMS.DBUpdater.DataScripts.REFRESH01.sql", false);
			res.Response = "Refreshed views and sprocs";
			res.RanUpdate = true;

			SetDbSchemaVersion(DatabaseUpdate.DbVersion02);
			return res;
		}

		private List<string> SplitScriptAtGo(string sqlQuery) {
			sqlQuery += "\r\n\r\nGO\r\n\r\n";
			sqlQuery = sqlQuery.Replace("\r\n", "\n");

			string[] splitcommands = sqlQuery.Split(new string[] { "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
			List<string> commandList = new List<string>(splitcommands);
			return commandList;
		}

		#region Execute SQL statements

		public Exception ExecScriptContents(string scriptContents, bool bIgnoreErr) {
			string _connStr = SetConn();

			return ExecScriptContents(_connStr, scriptContents, bIgnoreErr);
		}

		public Exception ExecScriptContents(string connectionString, string scriptContents, bool bIgnoreErr) {
			return ExecNonQuery(connectionString, scriptContents, bIgnoreErr);
		}

		private Exception ExecFileContents(string resouceName, bool bIgnoreErr) {
			string _connStr = SetConn();

			return ExecFileContents(_connStr, resouceName, bIgnoreErr);
		}

		private Exception ExecFileContents(string connectionString, string resourceName, bool ignoreErr) {
			string scriptContents = ReadEmbededScript(resourceName);

			Exception response = ExecScriptContents(connectionString, scriptContents, ignoreErr);

			return response;
		}

		#endregion Execute SQL statements

		#region Work with data keys

		private static object schemaCheckLocker = new object();

		//private static string SchemaKey = "cms_GetDbSchemaVersion";

		public static DataInfo GetDbSchemaVersion() {
			DataInfo di = null;
			lock (schemaCheckLocker) {

				di = GetDataKeyValue("DBSchema");

				return di;
			}
		}

		public static void SetDbSchemaVersion(string dataKeyValue) {
			SetDataKeyValue("DBSchema", dataKeyValue);
		}

		public static DataInfo GetDataKeyValue(string dataKeyName) {
			string _connStr = SetConn();

			DataInfo d = new DataInfo();

			SQLUpdateNugget n = SQLUpdateNugget.GetNuggets("SchemaVersionCheck").FirstOrDefault();
			if (n != null) {
				List<SqlParameter> parms = new List<SqlParameter>();

				SqlParameter parmKey = new SqlParameter();
				parmKey.ParameterName = "@DataKey";
				parmKey.SqlDbType = SqlDbType.VarChar;
				parmKey.Size = 100;
				parmKey.Direction = ParameterDirection.Input;
				parmKey.Value = dataKeyName;

				parms.Add(parmKey);

				DataTable dt = ExecuteDataTableCommands(_connStr, n.SQLQuery, parms);

				if (dt.Rows.Count > 0) {
					DataRow dr = dt.Rows[0];

					d.DataKey = dr["DataKey"].ToString();
					d.DataValue = dr["DataValue"].ToString();
				}
			}

			if (d != null && string.IsNullOrEmpty(d.DataValue)) {
				d.DataValue = string.Empty;
			}

			return d;
		}

		public static void SetDataKeyValue(string dataKeyName, string dataKeyValue) {
			string _connStr = SetConn();

			SQLUpdateNugget n = SQLUpdateNugget.GetNuggets("SchemaVersionUpdate").FirstOrDefault();

			if (n != null) {
				List<SqlParameter> parms = new List<SqlParameter>();

				SqlParameter parmNewVal = new SqlParameter();
				parmNewVal.ParameterName = "@DataValue";
				parmNewVal.SqlDbType = SqlDbType.VarChar;
				parmNewVal.Size = 100;
				parmNewVal.Direction = ParameterDirection.Input;
				parmNewVal.Value = dataKeyValue;

				parms.Add(parmNewVal);

				SqlParameter parmKey = new SqlParameter();
				parmKey.ParameterName = "@DataKey";
				parmKey.SqlDbType = SqlDbType.VarChar;
				parmKey.Size = 100;
				parmKey.Direction = ParameterDirection.Input;
				parmKey.Value = dataKeyName;

				parms.Add(parmKey);

				ExecuteNonQueryCommands(_connStr, n.SQLQuery, parms);
			}
		}

		#endregion Work with data keys

		#region General database routines

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
						WriteDebugException("execnonquery-ignore", ex);
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
							WriteDebugException("execnonquery", ex);
						}
					}
					exc = new Exception(sb.ToString());
					cn.Close();
				}
			}

			return exc;
		}

		private static void ExecuteNonQueryCommands(string connectionString, string sqlQuery, List<SqlParameter> SqlParms) {
			DataTable dt = new DataTable();

			using (SqlConnection cn = new SqlConnection(connectionString)) {
				cn.Open();
				using (SqlCommand cmd = new SqlCommand(sqlQuery, cn)) {
					cmd.CommandType = CommandType.Text;

					foreach (var p in SqlParms) {
						cmd.Parameters.Add(p);
					}

					int ret = cmd.ExecuteNonQuery();
				}
				cn.Close();
			}
		}

		private static DataTable ExecuteDataTableCommands(string connectionString, string sqlQuery, List<SqlParameter> SqlParms) {
			DataTable dt = new DataTable();

			using (SqlConnection cn = new SqlConnection(connectionString)) {
				using (SqlCommand cmd = new SqlCommand(sqlQuery, cn)) {
					cn.Open();
					cmd.CommandType = CommandType.Text;

					if (SqlParms != null) {
						foreach (var p in SqlParms) {
							cmd.Parameters.Add(p);
						}
					}

					using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
						da.Fill(dt);
					}
				}
				cn.Close();
			}

			return dt;
		}

		public static DataTable GetDataTable(string sqlQuery) {
			string _connStr = SetConn();

			return GetDataTable(_connStr, sqlQuery);
		}

		private static DataTable GetDataTable(string connectionString, string sqlQuery) {
			DataTable dt = new DataTable();

			using (SqlConnection cn = new SqlConnection(connectionString)) {
				using (SqlCommand cmd = new SqlCommand(sqlQuery, cn)) {
					cn.Open();
					cmd.CommandType = CommandType.Text;
					using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
						da.Fill(dt);
					}
					cn.Close();
				}
			}

			return dt;
		}

		public static DataTable GetTestData(string sqlQuery) {
			return GetTestData(sqlQuery, null);
		}

		public static DataTable GetTestData(string sqlQuery, List<SqlParameter> SqlParms) {
			string _connStr = SetConn();

			return GetTestData(_connStr, sqlQuery, SqlParms);
		}

		public static DataTable GetTestData(string connectionString, string sqlQuery, List<SqlParameter> SqlParms) {
			DataTable dt = new DataTable();

			try {
				using (SqlConnection cn = new SqlConnection(connectionString)) {
					cn.Open(); // throws if invalid

					DatabaseUpdate.FailedSQL = false;

					using (SqlCommand cmd = cn.CreateCommand()) {
						cmd.CommandText = sqlQuery;

						if (SqlParms != null) {
							foreach (var p in SqlParms) {
								cmd.Parameters.Add(p);
							}
						}

						using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
							da.Fill(dt);
						}
					}

					cn.Close();
				}
				DatabaseUpdate.LastSQLError = null;
			} catch (SqlException sqlEx) {
				DatabaseUpdate.LastSQLError = sqlEx;
				DatabaseUpdate.FailedSQL = true;
				WriteDebugException("gettestdata", sqlEx);
			}

			return dt;
		}

		private static DataSet GetDataSet(string sqlQuery) {
			string _connStr = SetConn();

			return GetDataSet(_connStr, sqlQuery);
		}

		private static DataSet GetDataSet(string connectionString, string sqlQuery) {
			DataSet ds = new DataSet();

			using (SqlConnection cn = new SqlConnection(connectionString)) {
				using (SqlCommand cmd = new SqlCommand(sqlQuery, cn)) {
					cn.Open();
					cmd.CommandType = CommandType.Text;
					using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
						da.Fill(ds);
					}
					cn.Close();
				}
			}

			return ds;
		}

		#endregion General database routines
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
		public string DataKey { get; set; }
		public string DataValue { get; set; }
	}

	//======================
	public class DatabaseUpdateMessage {
		public string Message { get; set; }
		public string ExceptionText { get; set; }
		public string InnerExceptionText { get; set; }
		public string Response { get; set; }
		public int Order { get; set; }
		public bool AlteredData { get; set; }
		public bool HasException { get; set; }

		public DatabaseUpdateMessage() {
			this.ExceptionText = null;
			this.InnerExceptionText = null;
			this.AlteredData = false;
			this.HasException = false;
			this.Message = "";
			this.Response = "";
			this.Order = -1;
		}
	}

	//======================
	public class DatabaseUpdateResponse {
		public Exception LastException { get; set; }
		public string Response { get; set; }
		public bool RanUpdate { get; set; }

		public DatabaseUpdateResponse() {
			this.LastException = null;
			this.Response = "";
			this.RanUpdate = false;
		}
	}
}