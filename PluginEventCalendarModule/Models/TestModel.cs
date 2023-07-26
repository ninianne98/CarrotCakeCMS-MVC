using System.Web;

namespace CarrotCake.CMS.Plugins.EventCalendarModule.Models {

	public class TestModel {

		public TestModel() { }

		public CalendarDisplaySettings Settings { get; set; }

		public HtmlString RenderedContent { get; set; }

		public CalendarViewModel ViewModel { get; set; }
	}
}