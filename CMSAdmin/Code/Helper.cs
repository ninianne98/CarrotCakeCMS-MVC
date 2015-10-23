using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

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

		public enum ControlLocation {
			PublicFooter,
			PopupFooter,
			MainFooter,
		}

		public static string GetAdminFooterView(ControlLocation CtrlKey) {
			string sControlPath = String.Empty;
			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();

			switch (CtrlKey) {
				case ControlLocation.PublicFooter:
					sControlPath = config.AdminFooterControls.ControlPathPublic;
					break;

				case ControlLocation.PopupFooter:
					sControlPath = config.AdminFooterControls.ControlPathPopup;
					break;

				case ControlLocation.MainFooter:
					sControlPath = config.AdminFooterControls.ControlPathMain;
					break;
			}

			return sControlPath;
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
	}
}