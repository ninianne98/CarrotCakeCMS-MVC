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

		public static string CurrentDbVersion { get { return DbVersion02B; } }

		public static string DbVersion00 { get { return "20150829"; } }

		public static string DbVersion01 { get { return "20151001"; } }

		public static string DbVersion02 { get { return "20200915"; } }

		public static string DbVersion02B { get { return "20200925"; } }

		internal static string ReadEmbededScript(string resouceName) {
			var sb = new StringBuilder();

			var assembly = Assembly.GetExecutingAssembly();
			var a_name = assembly.GetName().Name;

			if (resouceName.ToLowerInvariant().StartsWith(a_name.ToLowerInvariant()) == false) {
				resouceName = string.Format("{0}.DataScripts.{1}", a_name, resouceName);
			}

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

		public static bool SystemNeedsChecking(this Exception ex) {
			//assumption is database is probably empty / needs updating, so trigger the under construction view

			WriteDebugException("systemneedschecking", ex);

			if (ex is SqlException && ex != null) {
				string msg = ex.CombineMessage().ToLowerInvariant();
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

		private static object _logLocker = new object();

		public static void WriteDebugException(string debugSource, Exception objErr) {
			WriteDebugException(false, debugSource, objErr);
		}

		public static void WriteDebugException(bool bWriteError, string debugSource, Exception objErr) {
#if DEBUG
			bWriteError = true; // always write errors when debug build
#endif

			if (bWriteError && objErr != null) {
				var sb = new StringBuilder();

				sb.AppendLine("----------------  " + debugSource.ToUpperInvariant() + " - " + DateTime.Now.ToString() + "  ----------------");
				sb.AppendLine("[" + objErr.GetType().ToString() + "] " + objErr.Message);

				if (objErr.StackTrace != null) {
					sb.AppendLine(objErr.StackTrace);
				}

				if (objErr.InnerException != null) {
					sb.AppendLine(objErr.InnerException.Message);

					if (objErr.InnerException.Message != null) {
						sb.AppendLine(objErr.InnerException.Message);
					}
				}

				Encoding encode = Encoding.Default;

				string filePath = HttpContext.Current.Server.MapPath("~/carrot_errors.txt");

				lock (_logLocker) {
					using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)) {
						using (var sw = new StreamWriter(fs, encode)) {
							sw.Write(sb.ToString());
						}
					}
				}
			}
		}

		private static string _contentSqlStateKey = "cms_SqlTablesIncomplete";

		public static bool TablesIncomplete {
			get {
				string tablesIncomplete = string.Empty;
				bool isIncomplete = true;
				var ret = GetCacheItemString(_contentSqlStateKey);

				if (ret != null) {
					tablesIncomplete = ret;
				} else {
					try {
						var du = new DatabaseUpdate();
						isIncomplete = du.DatabaseNeedsUpdate();
					} catch (Exception ex) {
						isIncomplete = false;
						WriteDebugException("tablesincomplete", ex);
					}

					tablesIncomplete = isIncomplete.ToString();
					HttpContext.Current.Cache.Insert(_contentSqlStateKey, tablesIncomplete, null, DateTime.Now.AddMinutes(3), Cache.NoSlidingExpiration);
				}

				isIncomplete = Convert.ToBoolean(tablesIncomplete);
				return isIncomplete;
			}
		}

		public static void ResetSQLState() {
			var ret = GetCacheItem(_contentSqlStateKey);
			if (ret != null) {
				HttpContext.Current.Cache.Remove(_contentSqlStateKey);
			}
		}

		#region Work with data keys

		private static object schemaCheckLocker = new object();

		public static DataInfo GetDbSchemaVersion() {
			var di = DataInfo.CreateBlankSchema();
			lock (schemaCheckLocker) {
				try {
					di = GetDataKeyValue(DataInfo.DBSchema);
				} catch (Exception ex) {
					di = DataInfo.CreateBlankSchema();
					WriteDebugException("getdbschemaversion", ex);
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
			var msgTop = ex.Message + Environment.NewLine + ex.StackTrace;

			if (ex.InnerException != null) {
				msgInner = ex.InnerException.Message + Environment.NewLine + ex.InnerException.StackTrace;
				msgInner = Environment.NewLine + msgInner;
			}

			return msgTop + msgInner;
		}
	}
}