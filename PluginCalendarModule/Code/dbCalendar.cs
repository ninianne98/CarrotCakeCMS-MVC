using System.ComponentModel.DataAnnotations;
using System.Configuration;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace CarrotCake.CMS.Plugins.CalendarModule.Code {

	public partial class dbCalendarDataContext {
		private static string connString = ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"].ConnectionString;

		public static dbCalendarDataContext GetDataContext() {
			return GetDataContext(connString);
		}

		public static dbCalendarDataContext GetDataContext(string connection) {
			return new dbCalendarDataContext(connection);
		}
	}

	//================

	[MetadataType(typeof(ICalendar))]
	public partial class tblCalendar : ICalendar {
	}
}