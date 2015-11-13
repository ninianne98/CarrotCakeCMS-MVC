using Carrotware.CMS.Core;
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

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class WidgetListModel {

		public WidgetListModel() {
			this.Controls = new List<Widget>();
		}

		public WidgetListModel(Guid guidContentID) {
			this.Root_ContentID = guidContentID;
			this.PlaceholderName = String.Empty;

			using (WidgetHelper widgetHelper = new WidgetHelper()) {
				this.Controls = widgetHelper.GetWidgets(guidContentID, false);
			}
		}

		public Guid Root_ContentID { get; set; }

		public string PlaceholderName { get; set; }

		public List<Widget> Controls { get; set; }

		private List<CMSPlugin> _plugins = null;

		public List<CMSPlugin> Plugins {
			get {
				if (_plugins == null) {
					using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
						_plugins = cmsHelper.ToolboxPlugins;
					}
				}

				return _plugins;
			}
		}

		public string GetCaption(string controlPath) {
			CMSPlugin plug = (from p in this.Plugins
							  where p.FilePath.ToLower() == controlPath.ToLower()
							  select p).FirstOrDefault();

			if (plug != null) {
				return plug.Caption;
			}

			return "NONE";
		}
	}
}