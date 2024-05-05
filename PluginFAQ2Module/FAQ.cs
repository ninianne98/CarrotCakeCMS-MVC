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

namespace CarrotCake.CMS.Plugins.FAQ2 {

	partial class FAQDataContext {
		private static string connString = ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"].ConnectionString;

		public static FAQDataContext GetDataContext() {
			return GetDataContext(connString);
		}

		public static FAQDataContext GetDataContext(string connection) {
			return new FAQDataContext(connection);
		}
	}

	//=================

	[MetadataType(typeof(IFaqItem))]
	public partial class carrot_FaqItem : IFaqItem {
	}

	[MetadataType(typeof(IFaqCategory))]
	public partial class carrot_FaqCategory : IFaqCategory {
	}
}