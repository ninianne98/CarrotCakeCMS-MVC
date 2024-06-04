using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
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

	public abstract class FormSettingBase : IFormSettingBase {

		public FormSettingBase() {
			this.ValidationFailText = "Failed to validate as a human.";
			this.Uri = HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"].ToString();
		}

		public string PostPartialName { get; set; }
		public string Uri { get; set; }
		public bool UseValidateHuman { get; set; }
		public string ValidateHumanClass { get; set; }
		public string ValidationFailText { get; set; }

		public void GetSettingFromConfig(FormConfigBase config) {
			if (config != null && config.ValidateHuman != null) {
				this.UseValidateHuman = true;
				this.ValidateHumanClass = config.ValidateHuman.GetType().AssemblyQualifiedName;
				if (!string.IsNullOrEmpty(config.ValidateHuman.AltValidationFailText)) {
					this.ValidationFailText = config.ValidateHuman.AltValidationFailText;
				}
			} else {
				this.UseValidateHuman = false;
				this.ValidateHumanClass = string.Empty;
				this.ValidationFailText = string.Empty;
			}
		}

		public void SetHuman(IValidateHuman validateHuman) {
			if (validateHuman != null) {
				this.UseValidateHuman = true;
				this.ValidateHumanClass = validateHuman.GetType().AssemblyQualifiedName;
				if (!string.IsNullOrEmpty(validateHuman.AltValidationFailText)) {
					this.ValidationFailText = validateHuman.AltValidationFailText;
				}
			} else {
				this.UseValidateHuman = false;
				this.ValidateHumanClass = string.Empty;
				this.ValidationFailText = string.Empty;
			}
		}
	}

	//==============================

	public interface IFormSettingBase : IFormSettingRootBase {
		bool UseValidateHuman { get; set; }
		string ValidateHumanClass { get; set; }
		string ValidationFailText { get; set; }
	}

	//==============================

	public abstract class FormConfigRootBase : IFormConfigRootBase {

		public FormConfigRootBase() {
			this.PostPartialName = string.Empty;
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

	public abstract class FormConfigBase : IFormConfigBase {

		public FormConfigBase() {
			this.ValidateHuman = null;
			this.PostPartialName = string.Empty;
		}

		public FormConfigBase(string partialName)
			: this() {
			this.PostPartialName = partialName;
		}

		public FormConfigBase(string partialName, IValidateHuman validateHuman)
			: this(partialName) {
			this.ValidateHuman = validateHuman;
		}

		public string PostPartialName { get; set; }

		public IValidateHuman ValidateHuman { get; set; }
	}

	//==============================

	public interface IFormConfigBase : IFormConfigRootBase {
		IValidateHuman ValidateHuman { get; set; }
	}

	//==============================

	public interface IFormModelBase<S> {
		string EncodedSettings { get; set; }
		IValidateHuman ValidateHuman { get; set; }
		object ValidateSettings { get; set; }
		string ValidationValue { get; set; }

		S Settings { get; set; }

		void GetSettings();

		string SerializeSettings();
	}

	//==============================

	public abstract class FormModelBase<P> : IFormModelBase<P> {

		public FormModelBase() { }

		public string EncodedSettings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public object ValidateSettings { get; set; }
		public string ValidationValue { get; set; }

		public virtual P Settings { get; set; }

		public virtual void GetSettings() {
			this.ValidateSettings = null;
			Type type = typeof(P);  //  this.Settings.GetType();

			if (!string.IsNullOrEmpty(this.EncodedSettings)) {
				string xml = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				var xmlSerializer = new XmlSerializer(type);
				using (var sr = new StringReader(xml)) {
					this.ValidateSettings = xmlSerializer.Deserialize(sr);
				}

				if (this.ValidateSettings != null && this.ValidateSettings is IFormSettingBase) {
					IFormSettingBase settings = this.ValidateSettings as IFormSettingBase;

					if (!string.IsNullOrEmpty(settings.ValidateHumanClass)) {
						Type objType = ReflectionUtilities.GetTypeFromString(settings.ValidateHumanClass);
						object obj = Activator.CreateInstance(objType);
						this.ValidateHuman = (IValidateHuman)obj;
						this.ValidateHuman.AltValidationFailText = settings.ValidationFailText;
					}
				}
			}
		}

		public virtual string SerializeSettings() {
			Type type = typeof(P);  //  this.Settings.GetType();
			string xml = string.Empty;

			var xmlSerializer = new XmlSerializer(type);
			using (var sw = new StringWriter()) {
				xmlSerializer.Serialize(sw, this.Settings);
				xml = sw.ToString();
			}

			xml = xml.EncodeBase64();
			this.EncodedSettings = xml;

			return xml;
		}

		public virtual void WriteCache<T>(AjaxHelper helper, HtmlHelper<T> specialForm) where T : IFormModelBase<P> {
			string frmTag = Environment.NewLine
							+ specialForm.AntiForgeryToken().ToString()
							+ Environment.NewLine
							+ specialForm.HiddenFor(x => x.EncodedSettings).ToString()
							+ Environment.NewLine;

			helper.ViewContext.Writer.Write(frmTag);
		}
	}
}