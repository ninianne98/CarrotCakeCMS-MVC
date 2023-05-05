using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarrotCake.CMS.Plugins.CalendarModule.Models {

	public class TestModel {

		public TestModel() { }

		public CalendarDisplaySettings Settings { get; set; }

		public HtmlString RenderedContent { get; set; }

		public CalendarViewModel ViewModel { get; set; }
	}
}