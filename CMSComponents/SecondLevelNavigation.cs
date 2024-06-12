using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
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

	public class SecondLevelNavigation : SimpleListSortable {

		[Widget(WidgetAttribute.FieldMode.CheckBox)]
		public bool IncludeParent { get; set; }

		public override void LoadData() {
			base.LoadData();

			try {
				string sFoundVal = this.GetParmValue("IncludeParent", string.Empty);

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.IncludeParent = Convert.ToBoolean(sFoundVal);
				}
			} catch (Exception ex) { }

			List<SiteNav> lstNav = this.CmsPage.SiblingNav;

			if (this.IncludeParent) {
				if (lstNav != null && lstNav.Any()) {
					SiteNav p = this.CmsPage.ParentNav;
					if (p != null) {
						p.NavOrder = -100;
						lstNav.Add(p);
					}
				}
			}

			this.NavigationData = lstNav.OrderBy(ct => ct.NavMenuText).OrderBy(ct => ct.NavOrder).ToList();
		}
	}
}