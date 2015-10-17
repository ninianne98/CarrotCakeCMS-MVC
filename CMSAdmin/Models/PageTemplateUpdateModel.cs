using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;

using System.Linq;

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class PageTemplateUpdateModel : PageIndexModel {

		public PageTemplateUpdateModel()
			: base() {
			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				this.SiteTemplateList = cmsHelper.Templates;
			}
		}

		public List<CMSTemplate> SiteTemplateList { get; set; }

		public string SelectedTemplate { get; set; }
	}
}