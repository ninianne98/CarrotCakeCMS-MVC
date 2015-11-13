using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;

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

	public class PostTemplateUpdateModel : PostIndexModel {

		public PostTemplateUpdateModel()
			: base() {
			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				this.SiteTemplateList = cmsHelper.Templates;
			}
		}

		public List<CMSTemplate> SiteTemplateList { get; set; }

		public string SelectedTemplate { get; set; }
	}
}