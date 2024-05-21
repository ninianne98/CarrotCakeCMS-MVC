﻿using Carrotware.CMS.Core;
using System;

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

	public class ContentSingleModel {

		public ContentSingleModel() {
			this.Mode = SiteData.RawMode;
		}

		public Guid PageId { get; set; }

		public Guid? WidgetId { get; set; }

		public string Mode { get; set; } = SiteData.RawMode;

		public string Field { get; set; }

		public string PageText { get; set; }
	}
}