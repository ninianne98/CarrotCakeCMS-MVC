using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class FileUpModel {

		public FileUpModel() { }

		[DataType(DataType.Upload)]
		[Display(Name = "Posted File")]
		public HttpPostedFileBase PostedFile { get; set; }
	}
}