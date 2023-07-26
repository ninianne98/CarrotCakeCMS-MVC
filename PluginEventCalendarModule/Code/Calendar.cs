using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Drawing;

namespace CarrotCake.CMS.Plugins.EventCalendarModule.Code {
	public partial class CalendarDataContext {

		private static string connString = ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"].ConnectionString;

		public static CalendarDataContext GetDataContext() {

			return GetDataContext(connString);
		}


		public static CalendarDataContext GetDataContext(string connection) {

			return new CalendarDataContext(connection);
		}

	}

	//================

	[MetadataType(typeof(ICalendarEventProfile))]
	public partial class carrot_CalendarEventProfile : ICalendarEventProfile {

	}

	[MetadataType(typeof(ICalendarEventCategory))]
	public partial class carrot_CalendarEventCategory : ICalendarEventCategory, IValidatableObject {
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			List<ValidationResult> errors = new List<ValidationResult>();
			try {
				var bg = ColorTranslator.FromHtml(this.CategoryBGColor);
			} catch {
				ValidationResult err = new ValidationResult("CategoryBGColor is not a valid color", new string[] { "CategoryBGColor" });
				errors.Add(err);
			}
			try {
				var bg = ColorTranslator.FromHtml(this.CategoryFGColor);
			} catch {
				ValidationResult err = new ValidationResult("CategoryFGColor is not a valid color", new string[] { "CategoryFGColor" });
				errors.Add(err);
			}

			return errors;
		}
	}
}
