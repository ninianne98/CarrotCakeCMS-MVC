using System;
using System.ComponentModel;
using System.Configuration;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.Configuration;

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

	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CarrotCakeSectionGroup : ConfigurationSectionGroup {

		[ConfigurationProperty("Settings", IsRequired = true)]
		public CarrotCakeConfig Settings {
			get { return (CarrotCakeConfig)this.Sections["Settings"]; }
		}
	}

	//===============
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[SecuritySafeCritical]
	public class CarrotCakeConfig : ConfigurationSection {

		public static CarrotCakeConfig GetConfig() {
			return (CarrotCakeConfig)WebConfigurationManager.GetSection("CarrotCakeCMS.Web/Settings") ?? new CarrotCakeConfig();
		}

		[ConfigurationProperty("Config")]
		public MainConfigElement MainConfig {
			get {
				return (MainConfigElement)this["Config"];
			}
			set {
				this["Config"] = value;
			}
		}

		[ConfigurationProperty("FileManager")]
		public FileBrowserElement FileManagerConfig {
			get {
				return (FileBrowserElement)this["FileManager"];
			}
			set {
				this["FileManager"] = value;
			}
		}

		[ConfigurationProperty("Options")]
		public OptionsElement ExtraOptions {
			get {
				return (OptionsElement)this["Options"];
			}
			set {
				this["Options"] = value;
			}
		}

		[ConfigurationProperty("AdminFooter")]
		public AdminFooterElement AdminFooterControls {
			get {
				return (AdminFooterElement)this["AdminFooter"];
			}
			set {
				this["AdminFooter"] = value;
			}
		}

		[ConfigurationProperty("PublicSite")]
		public PublicSiteElement PublicSiteControls {
			get {
				return (PublicSiteElement)this["PublicSite"];
			}
			set {
				this["PublicSite"] = value;
			}
		}

		[ConfigurationProperty("OverrideConfigFile")]
		public ConfigFileElement ConfigFileLocation {
			get {
				return (ConfigFileElement)this["OverrideConfigFile"];
			}
			set {
				this["OverrideConfigFile"] = value;
			}
		}
	}

	//==============================
	public class MainConfigElement : ConfigurationElement {

		[Description("Site identity")]
		[ConfigurationProperty("SiteID", DefaultValue = null, IsRequired = false)]
		public Guid? SiteID {
			get {
				if (this["SiteID"] != null) {
					return new Guid(this["SiteID"].ToString());
				} else {
					return null;
				}
			}
			set {
				if (this["SiteID"] != null) {
					this["SiteID"] = value.ToString();
				} else {
					this["SiteID"] = null;
				}
			}
		}

		[Description("Override parameter for admin folder")]
		[ConfigurationProperty("AdminFolderPath", DefaultValue = "/c3-admin/", IsRequired = false)]
		public String AdminFolderPath {
			get { return (String)this["AdminFolderPath"]; }
			set { this["AdminFolderPath"] = value; }
		}
	}

	//==============================
	public class FileBrowserElement : ConfigurationElement {

		[Description("File extensions to block from the CMS file browser")]
		[ConfigurationProperty("BlockedExtensions", DefaultValue = null, IsRequired = false)]
		public String BlockedExtensions {
			get { return (String)this["BlockedExtensions"]; }
			set { this["BlockedExtensions"] = value; }
		}
	}

	//==============================
	public class OptionsElement : ConfigurationElement {

		[Description("Indicates if error log should be written to")]
		[ConfigurationProperty("WriteErrorLog", DefaultValue = false, IsRequired = false)]
		public bool WriteErrorLog {
			get { return (bool)this["WriteErrorLog"]; }
			set { this["WriteErrorLog"] = value; }
		}
	}

	//==============================
	public class ConfigFileElement : ConfigurationElement {

		[ConfigurationProperty("SiteMapping", DefaultValue = "SiteMapping.config", IsRequired = false)]
		public String SiteMapping {
			get { return (String)this["SiteMapping"]; }
			set { this["SiteMapping"] = value; }
		}

		[ConfigurationProperty("TextContentProcessors", DefaultValue = "TextContentProcessors.config", IsRequired = false)]
		public String TextContentProcessors {
			get { return (String)this["TextContentProcessors"]; }
			set { this["TextContentProcessors"] = value; }
		}

		[ConfigurationProperty("TemplatePath", DefaultValue = "~/Views/Templates/", IsRequired = false)]
		public String TemplatePath {
			get { return (String)this["TemplatePath"]; }
			set { this["TemplatePath"] = value; }
		}

		[ConfigurationProperty("PluginPath", DefaultValue = "~/Views/", IsRequired = false)]
		public String PluginPath {
			get { return (String)this["PluginPath"]; }
			set { this["PluginPath"] = value; }
		}
	}

	//==============================
	public class AdminFooterElement : ConfigurationElement {

		[ConfigurationProperty("ViewPathMain", DefaultValue = null, IsRequired = false)]
		public String ViewPathMain {
			get { return (String)this["ViewPathMain"]; }
			set { this["ViewPathMain"] = value; }
		}

		[ConfigurationProperty("ViewPathPopup", DefaultValue = null, IsRequired = false)]
		public String ViewPathPopup {
			get { return (String)this["ViewPathPopup"]; }
			set { this["ViewPathPopup"] = value; }
		}

		[ConfigurationProperty("ViewPathPublic", DefaultValue = null, IsRequired = false)]
		public String ViewPathPublic {
			get { return (String)this["ViewPathPublic"]; }
			set { this["ViewPathPublic"] = value; }
		}
	}

	//==============================
	public class PublicSiteElement : ConfigurationElement {

		[ConfigurationProperty("ViewPathHeader", DefaultValue = null, IsRequired = false)]
		public String ViewPathHeader {
			get { return (String)this["ViewPathHeader"]; }
			set { this["ViewPathHeader"] = value; }
		}

		[ConfigurationProperty("ViewPathFooter", DefaultValue = null, IsRequired = false)]
		public String ViewPathFooter {
			get { return (String)this["ViewPathFooter"]; }
			set { this["ViewPathFooter"] = value; }
		}
	}
}