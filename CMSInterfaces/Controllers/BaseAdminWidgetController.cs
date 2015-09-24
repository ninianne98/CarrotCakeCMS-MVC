using System;

namespace Carrotware.CMS.Interface.Controllers {

	public class BaseAdminWidgetController : BaseWidgetController, IAdminModule, IAdminModuleLoad {

		public BaseAdminWidgetController()
			: base() { }

		public BaseAdminWidgetController(IAdminModule data) {
			LoadData(data);
		}

		public Guid SiteID { get; set; }

		public Guid ModuleID { get; set; }

		public string ModuleName { get; set; }

		public string QueryStringFragment { get; set; }

		public string QueryStringPattern { get; set; }

		public void LoadData(IAdminModule data) {
			this.SiteID = data.SiteID;
			this.ModuleID = data.ModuleID;
			this.ModuleName = data.ModuleName;
			//TODO: more parms to go here...
		}
	}
}