using CarrotCake.CMS.Plugins.PhotoGallery.Models;
using Carrotware.CMS.Interface.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace CarrotCake.CMS.Plugins.PhotoGallery.Controllers {

	public class BasePublicController : BaseDataWidgetController {

		protected GalleryModel BuildModel(GallerySettings settings) {
			var model = new GalleryModel();

			if (settings != null) {
				model.GalleryID = settings.GalleryID;
				model.ShowHeading = settings.ShowHeading;
				model.ScaleImage = settings.ScaleImage;
				model.ThumbSize = settings.ThumbSize;
				model.PrettyPhotoSkin = settings.PrettyPhotoSkin;

				model.InstanceId = settings.WidgetClientID;

				using (var gh = new GalleryHelper(settings.SiteID)) {
					var gal = gh.GalleryGroupGetByID(settings.GalleryID);

					if (gal != null) {
						model.Gallery = gal;
						model.Images = (from g in gal.GalleryImages
										where g.GalleryID == settings.GalleryID
										orderby g.ImageOrder ascending
										select g).ToList();
					} else {
						model.Gallery = new GalleryGroup();
						model.Images = new List<GalleryImageEntry>();
					}
				}
			}

			return model;
		}
	}
}