using System.Configuration;
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

namespace Carrotware.CMS.Data {

	public partial class CarrotCMSDataContext {
		private static int iDBConnCounter = 0;

		private static string connString = ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"].ConnectionString;

		public static CarrotCMSDataContext Create() {
			return GetDataContext();
		}

		public static CarrotCMSDataContext Create(string connection) {
			return GetDataContext(connection);
		}

		public static CarrotCMSDataContext GetDataContext() {
			return GetDataContext(connString);
		}

		public static CarrotCMSDataContext GetDataContext(string connection) {
			var db = new CarrotCMSDataContext(connection);

			return DataContextCounter(db);
		}

		public static CarrotCMSDataContext Create(IDbConnection connection) {
			var db = new CarrotCMSDataContext(connection);

			return DataContextCounter(db);
		}

		protected static CarrotCMSDataContext DataContextCounter(CarrotCMSDataContext db) {
#if DEBUG
			DataDiagnostic dd = new DataDiagnostic(db, iDBConnCounter);
			iDBConnCounter++;
			if (iDBConnCounter > 4096) {
				iDBConnCounter = 0;
			}
			return db;
#else
			return db;
#endif
		}
	}
}