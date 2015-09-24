using Carrotware.CMS.DBUpdater;
using System;
using System.Collections.Generic;
using System.Linq;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class DatabaseSetupModel {

		public DatabaseSetupModel() {
			this.CreateUser = true;
			this.HasExceptions = false;
			this.Messages = new List<DatabaseUpdateMessage>();
		}

		public bool CreateUser { get; set; }
		public bool HasExceptions { get; set; }

		public List<DatabaseUpdateMessage> Messages { get; set; }
	}
}