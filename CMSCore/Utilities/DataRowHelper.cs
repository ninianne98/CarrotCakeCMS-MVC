using System;
using System.Data;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public static class DataRowHelper {

		public static string GetStringValue(this DataRow row, string columnName, string defaultValue = "") {
			if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value) {
				return row[columnName].ToString();
			}
			return defaultValue;
		}

		public static bool GetBoolValue(this DataRow row, string columnName, bool defaultValue = false) {
			if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value) {
				string val = row[columnName].ToString().ToLowerInvariant();
				if (val == "true" || val == "1" || val == "y" || val == "yes") {
					return true;
				}
				if (val == "false" || val == "0" || val == "n" || val == "no") {
					return false;
				}

				bool result;
				if (bool.TryParse(val, out result)) {
					return result;
				}
			}
			return defaultValue;
		}

		public static Guid GetGuidValue(this DataRow row, string columnName, Guid? defaultValue = null) {
			if (defaultValue == null) defaultValue = Guid.Empty;

			if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value) {
				string val = row[columnName].ToString();
				Guid result;
				if (Guid.TryParse(val, out result)) {
					return result;
				}
			}
			return defaultValue ?? Guid.Empty;
		}

		public static int GetIntValue(this DataRow row, string columnName, int defaultValue = 0) {
			if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value) {
				string val = row[columnName].ToString();
				int result;
				if (int.TryParse(val, out result)) {
					return result;
				}
			}
			return defaultValue;
		}
	}
}