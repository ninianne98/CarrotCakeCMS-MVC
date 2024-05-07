using System;
using System.Collections.Generic;
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
			return Create(connString);
		}

		public static CarrotCMSDataContext Create(string connection) {
#if DEBUG
			var db = new CarrotCMSDataContext(connection);
			DataDiagnostic dd = new DataDiagnostic(db, iDBConnCounter);
			iDBConnCounter++;
			if (iDBConnCounter > 4096) {
				iDBConnCounter = 0;
			}
			return db;
#else
			return new CarrotCMSDataContext(connection);
#endif

		}

		public static CarrotCMSDataContext Create(IDbConnection connection) {
#if DEBUG
			var db = new CarrotCMSDataContext(connection);
			DataDiagnostic dd = new DataDiagnostic(db, iDBConnCounter);
			iDBConnCounter++;
			if (iDBConnCounter > 4096) {
				iDBConnCounter = 0;
			}
			return db;
#else
			return new CarrotCMSDataContext(connection);
#endif

		}

		//public CarrotCMSDataContext() :
		//    base(global::Carrotware.CMS.Data.Properties.Settings.Default.CarrotwareCMSConnectionString, mappingSource) {
		//    OnCreated();
		//}

		//public CarrotCMSDataContext() :
		//    base(global::System.Configuration.ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"].ConnectionString, mappingSource) {
		//    OnCreated();
		//}
	}
}