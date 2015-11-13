using System;
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

namespace Carrotware.CMS.UI.Components {

	public class ChildNavigation : SimpleList {

		public override void LoadData() {
			base.LoadData();

			this.NavigationData = this.CmsPage.ChildNav;
		}
	}
}