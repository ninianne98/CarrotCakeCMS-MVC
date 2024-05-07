using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

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

	public class SQLUpdateNugget {

		public enum CompareMode {
			EQ,
			DNEQ,
			LT,
			GT,
			LTE,
			GTE,
		}

		public int RowCount { get; set; }
		public CompareMode Mode { get; set; }
		public int Priority { get; set; }
		public string AssociatedWith { get; set; }
		public string SQLQuery { get; set; }
		public bool AlwaysCheck { get; set; }

		protected static CompareMode GetModeType(string sVal) {
			CompareMode c = CompareMode.LT;
			if (!String.IsNullOrEmpty(sVal)) {
				c = (CompareMode)Enum.Parse(typeof(CompareMode), sVal, true);
			}
			return c;
		}

		private static object nuggetLocker = new Object();

		private static List<SQLUpdateNugget> _nuggets = null;

		public static List<SQLUpdateNugget> SQLNuggets {
			get {
				if (_nuggets == null) {
					lock (nuggetLocker) {
						_nuggets = new List<SQLUpdateNugget>();
						Assembly assembly = Assembly.GetExecutingAssembly();

						DataSet ds = new DataSet();
						string filePath = "Carrotware.CMS.DBUpdater.DatabaseChecks.xml";
						using (var stream = new StreamReader(assembly.GetManifestResourceStream(filePath))) {
							ds.ReadXml(stream);
						}

						_nuggets = (from d in ds.Tables[0].AsEnumerable()
									select new SQLUpdateNugget {
										AssociatedWith = d.Field<string>("testcontext"),
										SQLQuery = d.Field<string>("sql").Trim(),
										AlwaysCheck = Convert.ToBoolean(d.Field<string>("alwayscheck")),
										Mode = GetModeType(d.Field<string>("mode")),
										Priority = int.Parse(d.Field<string>("priority")),
										RowCount = int.Parse(d.Field<string>("rowcount"))
									}).OrderBy(x => x.Priority).ToList();

						_nuggets.RemoveAll(x => !x.SQLQuery.ToLower().Contains("select"));
					}
				}

				return _nuggets;
			}
		}

		public static bool EvalManditoryChecks() {
			List<SQLUpdateNugget> nugs = (from s in SQLNuggets
										  where s.AlwaysCheck == true
										  orderby s.Priority descending
										  select s).ToList();

			return RunEval(nugs);
		}

		public static bool EvalNuggetKey(string KeyName) {
			List<SQLUpdateNugget> nugs = GetNuggets(KeyName);

			return RunEval(nugs);
		}

		public static List<SQLUpdateNugget> GetNuggets(string KeyName) {
			List<SQLUpdateNugget> nugs = (from s in SQLNuggets
										  where s.AssociatedWith.ToLower().Contains("|" + KeyName.ToLower() + "|")
										  orderby s.Priority descending
										  select s).ToList();

			return nugs;
		}

		private static bool RunEval(List<SQLUpdateNugget> nugs) {
			foreach (var n in nugs) {
				DataTable table1 = DatabaseUpdate.GetTestData(n.SQLQuery);
				int iMatchCount = table1.Rows.Count;

				switch (n.Mode) {
					case CompareMode.DNEQ:
						if (iMatchCount != n.RowCount) {
							return true;
						}
						break;

					case CompareMode.EQ:
						if (iMatchCount == n.RowCount) {
							return true;
						}
						break;

					case CompareMode.LT:
						if (iMatchCount < n.RowCount) {
							return true;
						}
						break;

					case CompareMode.GT:
						if (iMatchCount > n.RowCount) {
							return true;
						}
						break;

					case CompareMode.LTE:
						if (iMatchCount <= n.RowCount) {
							return true;
						}
						break;

					case CompareMode.GTE:
						if (iMatchCount >= n.RowCount) {
							return true;
						}
						break;

					default:
						break;
				}
			}

			return false;
		}
	}
}