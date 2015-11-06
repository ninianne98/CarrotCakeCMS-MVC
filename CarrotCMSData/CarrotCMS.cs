using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
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
			CarrotCMSDataContext _db = new CarrotCMSDataContext(connection);
			DataDiagnostic dd = new DataDiagnostic(_db, iDBConnCounter);
			iDBConnCounter++;
			if (iDBConnCounter > 4096) {
				iDBConnCounter = 0;
			}
			return _db;
#endif
			return new CarrotCMSDataContext(connection);
		}

		public static CarrotCMSDataContext Create(IDbConnection connection) {
#if DEBUG
			CarrotCMSDataContext _db = new CarrotCMSDataContext(connection);
			DataDiagnostic dd = new DataDiagnostic(_db, iDBConnCounter);
			iDBConnCounter++;
			if (iDBConnCounter > 4096) {
				iDBConnCounter = 0;
			}
			return _db;
#endif
			return new CarrotCMSDataContext(connection);
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