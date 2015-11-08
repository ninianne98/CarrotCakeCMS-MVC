using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
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
* Dual licensed under the MIT or GPL Version 2 licenses.
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

		public static string InsertSpecialView(ViewLocation CtrlKey) {
			string sViewPath = String.Empty;
			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();

			switch (CtrlKey) {
				case ViewLocation.AdminPublicFooter:
					sViewPath = config.AdminFooterControls.ViewPathPublic;
					break;

				case ViewLocation.AdminPopupFooter:
					sViewPath = config.AdminFooterControls.ViewPathPopup;
					break;

				case ViewLocation.AdminMainFooter:
					sViewPath = config.AdminFooterControls.ViewPathMain;
					break;

				case ViewLocation.PublicMainHeader:
					sViewPath = config.PublicSiteControls.ViewPathHeader;
					break;

				case ViewLocation.PublicMainFooter:
					sViewPath = config.PublicSiteControls.ViewPathFooter;
					break;
			}

			return sViewPath;
		}

		public static Dictionary<bool, string> CreateBoolFilter() {
			var option = new Dictionary<bool, string>();
			option.Add(true, "Yes");
			option.Add(false, "No");

			return option;
		}

		public static void HandleErrorDict(ModelStateDictionary stateDictionary, Dictionary<string, string> validationsDictionary) {
			if (validationsDictionary.Any()) {
				stateDictionary.AddModelError(String.Empty, "Please review and correct the noted errors.");
			}

			foreach (KeyValuePair<string, string> valuePair in validationsDictionary) {
				stateDictionary.AddModelError(valuePair.Key, valuePair.Value);
			}
		}

		public static void HandleErrorDict(ModelStateDictionary stateDictionary) {
			List<string> keys = stateDictionary.Keys.Where(x => !String.IsNullOrEmpty(x)).ToList();

			foreach (string d in keys) {
				foreach (var err in stateDictionary[d].Errors) {
					stateDictionary.AddModelError(String.Empty, String.Format("{0}: {1}", d, err.ErrorMessage));
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
						errorMemberNames.Add(String.Empty);
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
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			ViewEngines.Engines.Add(new CarrotViewEngineTemplate());
			ViewEngines.Engines.Add(new CarrotViewEngineWidget());
			// ViewEngines.Engines.Add(new CarrotViewEngineWidgetAdmin());

			ControllerBuilder.Current.SetControllerFactory(CmsControllerFactory.GetFactory());
		}
	}
}