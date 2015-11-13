using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	[Serializable()]
	public class CMSAdminModule {

		public CMSAdminModule() {
			PluginMenus = new List<CMSAdminModuleMenu>();
		}

		public string AreaKey { get; set; }
		public string PluginName { get; set; }
		public List<CMSAdminModuleMenu> PluginMenus { get; set; }
	}

	[Serializable()]
	public class CMSAdminModuleMenu {
		public string AreaKey { get; set; }
		public int SortOrder { get; set; }
		public string Caption { get; set; }
		public string Action { get; set; }
		public string Controller { get; set; }
		public bool UsePopup { get; set; }
		public bool IsVisible { get; set; }
	}

	[Serializable()]
	public class CMSPlugin {

		public CMSPlugin() {
			this.SortOrder = 1000;
			this.SystemPlugin = false;
		}

		public bool SystemPlugin { get; set; }
		public int SortOrder { get; set; }
		public string FilePath { get; set; }
		public string Caption { get; set; }
	}

	[Serializable()]
	public class CMSTemplate {
		public string TemplatePath { get; set; }
		public string Caption { get; set; }
		public string EncodedPath { get; set; }
	}

	[Serializable()]
	public class CMSTextWidget {
		public string AssemblyString { get; set; }
		public string DisplayName { get; set; }
	}

	[Serializable()]
	public class CMSTextWidgetPicker {
		public Guid TextWidgetPickerID { get; set; }
		public string AssemblyString { get; set; }
		public string DisplayName { get; set; }
		public bool ProcessBody { get; set; }
		public bool ProcessPlainText { get; set; }
		public bool ProcessHTMLText { get; set; }
		public bool ProcessComment { get; set; }
		public bool ProcessSnippet { get; set; }
	}

	[Serializable()]
	public class DynamicSite {
		public Guid SiteID { get; set; }
		public string DomainName { get; set; }
	}

	[Serializable()]
	public class CMSFilePath {

		public CMSFilePath() {
			this.DateChecked = DateTime.UtcNow;
			this.FileExists = false;
			this.SiteID = Guid.Empty;
			this.TemplateFile = null;
		}

		public CMSFilePath(string fileName) {
			this.DateChecked = DateTime.UtcNow;
			this.TemplateFile = fileName.ToLower();
			this.SiteID = Guid.Empty;
			this.FileExists = File.Exists(HttpContext.Current.Server.MapPath(this.TemplateFile));
		}

		public CMSFilePath(string fileName, Guid siteID) {
			this.DateChecked = DateTime.UtcNow;
			this.TemplateFile = fileName.ToLower();
			this.SiteID = siteID;
			this.FileExists = File.Exists(HttpContext.Current.Server.MapPath(this.TemplateFile));
		}

		public DateTime DateChecked { get; set; }
		public string TemplateFile { get; set; }
		public bool FileExists { get; set; }
		public Guid SiteID { get; set; }

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is CMSFilePath) {
				CMSFilePath p = (CMSFilePath)obj;
				return (this.TemplateFile.ToLower() == p.TemplateFile.ToLower())
					&& (this.SiteID == p.SiteID);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return TemplateFile.ToLower().GetHashCode() ^ SiteID.GetHashCode();
		}
	}
}