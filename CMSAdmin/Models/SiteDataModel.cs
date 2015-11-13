using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class SiteDataModel : IValidatableObject {

		public SiteDataModel()
			: base() { }

		public SiteDataModel(SiteData model) {
			Load(model);
		}

		public bool CreateHomePage { get; set; }

		public SiteData Site { get; set; }

		public void Load(SiteData model) {
			this.Site = model;
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			List<ValidationResult> errors = new List<ValidationResult>();
			IEnumerable<ValidationResult> oldErrors = this.Site.Validate(validationContext);

			foreach (ValidationResult s in oldErrors) {
				List<string> mbrs = s.MemberNames.ToList().Select(m => String.Format("Site.{0}", m)).ToList();

				errors.Add(new ValidationResult(s.ErrorMessage, mbrs));
			}

			return errors;
		}
	}
}