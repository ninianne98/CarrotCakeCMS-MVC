using Carrotware.CMS.DBUpdater;
using System.Collections.Generic;
using System.Linq;

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

	public class DatabaseSetupModel {

		public DatabaseSetupModel() {
			this.CreateUser = true;
			this.HasExceptions = false;
			this.Messages = new List<DatabaseUpdateMessage>();
		}

		public void SetMessages(List<DatabaseUpdateMessage> lst) {
			this.Messages = lst != null ? lst : new List<DatabaseUpdateMessage>();
			this.HasExceptions = this.Messages.Where(x => !string.IsNullOrWhiteSpace(x.ExceptionText)).Any();
		}

		public bool CreateUser { get; set; } = true;
		public bool HasExceptions { get; set; } = false;

		public List<DatabaseUpdateMessage> Messages { get; set; } = new List<DatabaseUpdateMessage>();
	}
}