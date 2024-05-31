using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using Carrotware.CMS.UI.Components;
using Carrotware.Web.UI.Components;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin {

	public static class Helper {

		public enum ViewLocation {
			AdminPublicFooter,
			AdminPopupFooter,
			AdminMainFooter,
			PublicMainFooter,
			PublicMainHeader,
		}

		public static string WebServiceAddress {
			get {
				return CarrotCakeHtml.WebServiceAddress;
			}
		}

		public static CmsSkin.SkinOption _theme = CmsSkin.SkinOption.None;

		public static CmsSkin.SkinOption SiteSkin {
			get {
				if (_theme == CmsSkin.SkinOption.None) {
					var config = CarrotCakeConfig.GetConfig();
					string skin = config.MainConfig.SiteSkin;
					var actualSkin = CmsSkin.SkinOption.Classic;
					try { actualSkin = (CmsSkin.SkinOption)Enum.Parse(typeof(CmsSkin.SkinOption), skin, true); } catch { }

					_theme = actualSkin;
				}

				return _theme;
			}
		}

		public static string MainColorCode {
			get {
				return CmsSkin.GetPrimaryColorCode(SiteSkin);
			}
		}

		public static bool? _bootstrap = null;

		public static bool UseBootstrap {
			get {
				if (_bootstrap == null) {
					var config = CarrotCakeConfig.GetConfig();
					_bootstrap = config.MainConfig.UseBootstrap;
				}

				return _bootstrap.HasValue ? _bootstrap.Value : false;
			}
		}

		public static string InsertSpecialView(ViewLocation CtrlKey) {
			string viewPath = string.Empty;
			var config = CarrotCakeConfig.GetConfig();

			switch (CtrlKey) {
				case ViewLocation.AdminPublicFooter:
					viewPath = config.AdminFooterControls.ViewPathPublic;
					break;

				case ViewLocation.AdminPopupFooter:
					viewPath = config.AdminFooterControls.ViewPathPopup;
					break;

				case ViewLocation.AdminMainFooter:
					viewPath = config.AdminFooterControls.ViewPathMain;
					break;

				case ViewLocation.PublicMainHeader:
					viewPath = config.PublicSiteControls.ViewPathHeader;
					break;

				case ViewLocation.PublicMainFooter:
					viewPath = config.PublicSiteControls.ViewPathFooter;
					break;
			}

			return viewPath;
		}

		public static Dictionary<bool, string> CreateBoolFilter() {
			var option = new Dictionary<bool, string>();
			option.Add(true, "Yes");
			option.Add(false, "No");

			return option;
		}

		public static string ShortDateFormatPattern {
			get {
				return CarrotWeb.ShortDateFormatPattern;
			}
		}

		public static string ShortDateTimeFormatPattern {
			get {
				return CarrotWeb.ShortDateTimeFormatPattern;
			}
		}

		public static string ShortDatePattern {
			get {
				return CarrotWeb.ShortDatePattern;
			}
		}

		public static string ShortTimePattern {
			get {
				return CarrotWeb.ShortTimePattern;
			}
		}

		public static string ReadEmbededScript(string sResouceName) {
			return CarrotWeb.GetManifestResourceText(typeof(Controllers.CmsContentController), sResouceName);
		}

		public static byte[] ReadEmbededBinary(string sResouceName) {
			return CarrotWeb.GetManifestResourceBytes(typeof(Controllers.CmsContentController), sResouceName);
		}

		public static string GetWebResourceUrl(string sResouceName) {
			return CarrotWeb.GetWebResourceUrl(typeof(Controllers.CmsContentController), sResouceName);
		}

		public static void AddErrors(ModelStateDictionary stateDictionary, IdentityResult result) {
			foreach (var error in result.Errors) {
				stateDictionary.AddModelError(string.Empty, error);
			}
		}

		public static void HandleErrorDict(ModelStateDictionary stateDictionary, Dictionary<string, string> validationsDictionary) {
			if (validationsDictionary.Any()) {
				stateDictionary.AddModelError(string.Empty, "Please review and correct the noted errors.");
			}

			foreach (KeyValuePair<string, string> valuePair in validationsDictionary) {
				stateDictionary.AddModelError(valuePair.Key, valuePair.Value);
			}
		}

		public static void HandleErrorDict(ModelStateDictionary stateDictionary) {
			List<string> keys = stateDictionary.Keys.Where(x => !string.IsNullOrEmpty(x)).ToList();

			foreach (string d in keys) {
				foreach (var err in stateDictionary[d].Errors) {
					stateDictionary.AddModelError(string.Empty, string.Format("{0}: {1}", d, err.ErrorMessage));
				}
			}
		}

		public static void ForceValidation(ModelStateDictionary stateDictionary, Object m) {
			IValidatableObject model = null;

			if (m is IValidatableObject) {
				model = m as IValidatableObject;

				IEnumerable<ValidationResult> errors = model.Validate(new ValidationContext(model, null, null));

				List<string> modelStateKeys = stateDictionary.Keys.ToList();
				List<ModelState> modelStateValues = stateDictionary.Values.ToList();

				foreach (ValidationResult error in errors) {
					List<string> errorMemberNames = error.MemberNames.ToList();
					if (errorMemberNames.Count == 0) {
						errorMemberNames.Add(string.Empty);
					}

					foreach (string memberName in errorMemberNames) {
						int index = modelStateKeys.IndexOf(memberName);
						if (index < 0 || !modelStateValues[index].Errors.Any(i => i.ErrorMessage == error.ErrorMessage)) {
							stateDictionary.AddModelError(memberName, error.ErrorMessage);
						}
					}
				}
			}
		}

		public static void RegisterCmsComponents() {
			//AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			ViewEngines.Engines.Add(new CarrotViewEngineTemplate());
			ViewEngines.Engines.Add(new CarrotViewEngineWidget());
			// ViewEngines.Engines.Add(new CarrotViewEngineWidgetAdmin());

			ControllerBuilder.Current.SetControllerFactory(CmsControllerFactory.GetFactory());
		}
	}
}