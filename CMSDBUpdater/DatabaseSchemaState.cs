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

	public static class DatabaseSchemaState {
		public static SqlException LastSQLError { get; set; }

		public static string CurrentDbVersion { get { return DbVersion02; } }

		public static string DbVersion00 { get { return "20150829"; } }

		public static string DbVersion01 { get { return "20151001"; } }

		public static string DbVersion02 { get { return "20200915"; } }

		public static string ReadEmbededScript(string resouceName) {
			var sb = new StringBuilder();

			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = new StreamReader(assembly.GetManifestResourceStream(resouceName))) {
				sb.Append(stream.ReadToEnd());
			}

			return sb.ToString();
		}

		public static string SetConn() {
			string connectionString = string.Empty;
			string keyName = "CarrotwareCMSConnectionString";

			if (ConfigurationManager.ConnectionStrings[keyName] != null) {
				var csSetting = ConfigurationManager.ConnectionStrings[keyName];
				connectionString = csSetting.ConnectionString;
			}

			return connectionString;
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
				string msg = ex.Message.ToLowerInvariant();
				if (ex.InnerException != null) {
					msg += "\r\n" + ex.InnerException.Message.ToLowerInvariant();
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
			if (!DatabaseSchemaState.FailedSQL) {
				bool bTestResult = false;

				DataInfo ver = GetDbSchemaVersion();
				bTestResult = ver.DataValue != DatabaseSchemaState.CurrentDbVersion;
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

		public static bool UsersExist {
			get {
				if (!DatabaseSchemaState.FailedSQL) {
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

		#region Work with data keys

		private static object schemaCheckLocker = new object();

		//private static string SchemaKey = "cms_GetDbSchemaVersion";

		public static DataInfo GetDbSchemaVersion() {
			var di = DataInfo.CreateBlankSchema();
			lock (schemaCheckLocker) {
				try {
					di = GetDataKeyValue(DataInfo.DBSchema);
				} catch (Exception ex) {
					di = DataInfo.CreateBlankSchema();
				}
			}
			return di;
		}

		public static void SetDbSchemaVersion(string dataKeyValue) {
			SetDataKeyValue(DataInfo.DBSchema, dataKeyValue);
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

					DatabaseSchemaState.FailedSQL = false;

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
				DatabaseSchemaState.LastSQLError = null;
			} catch (SqlException sqlEx) {
				DatabaseSchemaState.LastSQLError = sqlEx;
				DatabaseSchemaState.FailedSQL = true;
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

	public static class DatabaseUpdateResponseExtensions {

		public static string CombineMessage(this Exception ex) {
			var msgInner = string.Empty;
			var msgTop = ex.Message + "\n" + ex.StackTrace;

			if (ex.InnerException != null) {
				msgInner = ex.InnerException.Message + "\n" + ex.InnerException.StackTrace;
				msgInner = "\n" + msgInner;
			}

			return msgTop + msgInner;
		}
	}
}