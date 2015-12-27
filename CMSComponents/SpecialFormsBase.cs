using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

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

	public abstract class FormSettingRootBase : IFormSettingRootBase {

		public FormSettingRootBase() {
			this.Uri = HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"].ToString();
		}

		public string PostPartialName { get; set; }
		public string Uri { get; set; }
	}

	//==============================

	public interface IFormSettingRootBase {
		string PostPartialName { get; set; }
		string Uri { get; set; }
	}

	//==============================

	public abstract class FormSettingBase : FormSettingRootBase, IFormSettingBase {

		public FormSettingBase()
			: base() {
			this.ValidationFailText = "Failed to validate as a human.";
		}

		public bool UseValidateHuman { get; set; }
		public string ValidateHumanClass { get; set; }
		public string ValidationFailText { get; set; }

		public void GetSettingFromConfig(FormConfigBase config) {
			if (config != null && config.ValidateHuman != null) {
				this.UseValidateHuman = true;
				this.ValidateHumanClass = config.ValidateHuman.GetType().AssemblyQualifiedName;
				if (!String.IsNullOrEmpty(config.ValidateHuman.AltValidationFailText)) {
					this.ValidationFailText = config.ValidateHuman.AltValidationFailText;
				}
			} else {
				this.UseValidateHuman = false;
				this.ValidateHumanClass = String.Empty;
				this.ValidationFailText = String.Empty;
			}
		}

		public void SetHuman(IValidateHuman validateHuman) {
			if (validateHuman != null) {
				this.UseValidateHuman = true;
				this.ValidateHumanClass = validateHuman.GetType().AssemblyQualifiedName;
				if (!String.IsNullOrEmpty(validateHuman.AltValidationFailText)) {
					this.ValidationFailText = validateHuman.AltValidationFailText;
				}
			} else {
				this.UseValidateHuman = false;
				this.ValidateHumanClass = String.Empty;
				this.ValidationFailText = String.Empty;
			}
		}
	}

	//==============================

	public interface IFormSettingBase {
		bool UseValidateHuman { get; set; }
		string ValidateHumanClass { get; set; }
		string ValidationFailText { get; set; }
	}

	//==============================

	public abstract class FormConfigRootBase : IFormConfigRootBase {

		public FormConfigRootBase() {
			this.PostPartialName = String.Empty;
		}

		public FormConfigRootBase(string partialName)
			: this() {
			this.PostPartialName = partialName;
		}

		public string PostPartialName { get; set; }
	}

	//==============================

	public interface IFormConfigRootBase {
		string PostPartialName { get; set; }
	}

	//==============================

	public abstract class FormConfigBase : FormConfigRootBase, IFormConfigBase {

		public FormConfigBase()
			: base() {
			this.ValidateHuman = null;
		}

		public FormConfigBase(string partialName)
			: base(partialName) {
		}

		public FormConfigBase(string partialName, IValidateHuman validateHuman)
			: base(partialName) {
			this.ValidateHuman = validateHuman;
		}

		public IValidateHuman ValidateHuman { get; set; }
	}

	//==============================

	public interface IFormConfigBase {
		IValidateHuman ValidateHuman { get; set; }
	}

	//==============================

	public abstract class FormModelBase {

		public FormModelBase() { }

		public string EncodedSettings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public Object ValidateSettings { get; set; }
		public string ValidationValue { get; set; }

		public void GetSettings(Type type) {
			this.ValidateSettings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(type);
				using (StringReader stringReader = new StringReader(sXML)) {
					this.ValidateSettings = xmlSerializer.Deserialize(stringReader);
				}

				if (this.ValidateSettings != null && this.ValidateSettings is IFormSettingBase) {
					IFormSettingBase settings = this.ValidateSettings as IFormSettingBase;

					if (!String.IsNullOrEmpty(settings.ValidateHumanClass)) {
						Type objType = ReflectionUtilities.GetTypeFromString(settings.ValidateHumanClass);
						Object obj = Activator.CreateInstance(objType);
						this.ValidateHuman = (IValidateHuman)obj;
						this.ValidateHuman.AltValidationFailText = settings.ValidationFailText;
					}
				}
			}
		}
	}
}