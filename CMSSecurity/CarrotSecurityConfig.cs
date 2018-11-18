using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

namespace Carrotware.CMS.Security {

	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CarrotSecuritySectionGroup : ConfigurationSectionGroup {

		[ConfigurationProperty("Settings", IsRequired = true)]
		public CarrotSecurityConfig Settings {
			get { return (CarrotSecurityConfig)this.Sections["Settings"]; }
		}
	}

	//===============
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[SecuritySafeCritical]
	public class CarrotSecurityConfig : ConfigurationSection {

		public static CarrotSecurityConfig GetConfig() {
			return (CarrotSecurityConfig)WebConfigurationManager.GetSection("CarrotSecurity/Settings") ?? new CarrotSecurityConfig();
		}

		[ConfigurationProperty("UserValidator")]
		public UserValidatorElement UserValidator {
			get {
				return (UserValidatorElement)this["UserValidator"];
			}
			set {
				this["UserValidator"] = value;
			}
		}

		[ConfigurationProperty("PasswordValidator")]
		public PasswordValidatorElement PasswordValidator {
			get {
				return (PasswordValidatorElement)this["PasswordValidator"];
			}
			set {
				this["PasswordValidator"] = value;
			}
		}

		[ConfigurationProperty("AdditionalSettings")]
		public AdditionalSettingsElement AdditionalSettings {
			get {
				return (AdditionalSettingsElement)this["AdditionalSettings"];
			}
			set {
				this["AdditionalSettings"] = value;
			}
		}
	}

	//==============================
	public class UserValidatorElement : ConfigurationElement {

		[ConfigurationProperty("AllowOnlyAlphanumericUserNames", DefaultValue = true, IsRequired = false)]
		public bool AllowOnlyAlphanumericUserNames {
			get { return (bool)this["AllowOnlyAlphanumericUserNames"]; }
			set { this["AllowOnlyAlphanumericUserNames"] = value; }
		}

		[ConfigurationProperty("RequireUniqueEmail", DefaultValue = true, IsRequired = false)]
		public bool RequireUniqueEmail {
			get { return (bool)this["RequireUniqueEmail"]; }
			set { this["RequireUniqueEmail"] = value; }
		}
	}

	//==============================
	public class PasswordValidatorElement : ConfigurationElement {

		[ConfigurationProperty("RequiredLength", DefaultValue = 8, IsRequired = false)]
		public int RequiredLength {
			get { return (int)this["RequiredLength"]; }
			set { this["RequiredLength"] = value; }
		}

		[ConfigurationProperty("RequireNonLetterOrDigit", DefaultValue = true, IsRequired = false)]
		public bool RequireNonLetterOrDigit {
			get { return (bool)this["RequireNonLetterOrDigit"]; }
			set { this["RequireNonLetterOrDigit"] = value; }
		}

		[ConfigurationProperty("RequireDigit", DefaultValue = true, IsRequired = false)]
		public bool RequireDigit {
			get { return (bool)this["RequireDigit"]; }
			set { this["RequireDigit"] = value; }
		}

		[ConfigurationProperty("RequireLowercase", DefaultValue = true, IsRequired = false)]
		public bool RequireLowercase {
			get { return (bool)this["RequireLowercase"]; }
			set { this["RequireLowercase"] = value; }
		}

		[ConfigurationProperty("RequireUppercase", DefaultValue = true, IsRequired = false)]
		public bool RequireUppercase {
			get { return (bool)this["RequireUppercase"]; }
			set { this["RequireUppercase"] = value; }
		}
	}

	//==============================
	public class AdditionalSettingsElement : ConfigurationElement {

		[ConfigurationProperty("MaxFailedAccessAttemptsBeforeLockout", DefaultValue = 5, IsRequired = false)]
		public int MaxFailedAccessAttemptsBeforeLockout {
			get { return (int)this["MaxFailedAccessAttemptsBeforeLockout"]; }
			set { this["MaxFailedAccessAttemptsBeforeLockout"] = value; }
		}

		[ConfigurationProperty("DefaultAccountLockoutTimeSpan", DefaultValue = 15, IsRequired = false)]
		public int DefaultAccountLockoutTimeSpan {
			get { return (int)this["DefaultAccountLockoutTimeSpan"]; }
			set { this["DefaultAccountLockoutTimeSpan"] = value; }
		}

		[ConfigurationProperty("UserLockoutEnabledByDefault", DefaultValue = true, IsRequired = false)]
		public bool UserLockoutEnabledByDefault {
			get { return (bool)this["UserLockoutEnabledByDefault"]; }
			set { this["UserLockoutEnabledByDefault"] = value; }
		}

		[ConfigurationProperty("DataProtectionProviderAppName", DefaultValue = "CarrotCake CMS", IsRequired = false)]
		public String DataProtectionProviderAppName {
			get { return (String)this["DataProtectionProviderAppName"]; }
			set { this["DataProtectionProviderAppName"] = value; }
		}

		[ConfigurationProperty("LoginPath", DefaultValue = "/c3-admin/Login", IsRequired = false)]
		public String LoginPath {
			get { return (String)this["LoginPath"]; }
			set { this["LoginPath"] = value; }
		}

		[ConfigurationProperty("ExpireTimeSpan", DefaultValue = 360, IsRequired = false)]
		public int ExpireTimeSpan {
			get { return (int)this["ExpireTimeSpan"]; }
			set { this["ExpireTimeSpan"] = value; }
		}

		[ConfigurationProperty("SetCookieExpireTimeSpan", DefaultValue = true, IsRequired = false)]
		public Boolean SetCookieExpireTimeSpan {
			get { return (Boolean)this["SetCookieExpireTimeSpan"]; }
			set { this["SetCookieExpireTimeSpan"] = value; }
		}

		[ConfigurationProperty("ValidateInterval", DefaultValue = 30, IsRequired = false)]
		public int ValidateInterval {
			get { return (int)this["ValidateInterval"]; }
			set { this["ValidateInterval"] = value; }
		}
	}
}