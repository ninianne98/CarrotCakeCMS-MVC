using System.Web.Mvc;

namespace CarrotCake.CMS.Plugins.PhotoGallery.Models {

	public class GalleryTest {
		public GallerySettings Settings { get; set; }

		public string RenderedContent { get; set; }

		public PartialViewResult PartialResult { get; set; }
	}
}