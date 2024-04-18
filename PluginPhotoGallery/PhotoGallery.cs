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

namespace CarrotCake.CMS.Plugins.PhotoGallery {
	public partial class PhotoGalleryDataContext {

		private static string connString = ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"].ConnectionString;

		public static PhotoGalleryDataContext GetDataContext() {

			return GetDataContext(connString);
		}


		public static PhotoGalleryDataContext GetDataContext(string connection) {

			return new PhotoGalleryDataContext(connection);
		}

	}
}
