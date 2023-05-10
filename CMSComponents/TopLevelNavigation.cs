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

	public class TopLevelNavigation : SimpleListSortable {

		public override void LoadData() {
			base.LoadData();

			this.NavigationData = this.CmsPage.TopNav;
		}
	}
}