using CarrotCake.CMS.Plugins.PhotoGallery.Models;
using Carrotware.CMS.Interface;
using Carrotware.CMS.Interface.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CarrotCake.CMS.Plugins.PhotoGallery.Controllers {

	public class HomeController : BaseDataWidgetController {

		public ActionResult Index() {
			return View();
		}

		[WidgetActionSettingModel("CarrotCake.CMS.Plugins.PhotoGallery.GallerySettings, CarrotCake.CMS.Plugins.PhotoGallery")]
		public PartialViewResult ShowPrettyPhotoGallery() {
			GallerySettings settings = new GallerySettings();

			if (WidgetPayload is GallerySettings) {
				settings = (GallerySettings)WidgetPayload;
				settings.LoadData();
			}

			GalleryModel model = new GalleryModel();

			if (settings != null) {
				model.GalleryID = settings.GalleryID;
				model.ShowHeading = settings.ShowHeading;
				model.ScaleImage = settings.ScaleImage;
				model.ThumbSize = settings.ThumbSize;
				model.PrettyPhotoSkin = settings.PrettyPhotoSkin;

				model.InstanceId = settings.WidgetClientID;

				GalleryHelper gh = new GalleryHelper(settings.SiteID);

				var gal = gh.GalleryGroupGetByID(model.GalleryID);

				if (gal != null) {
					model.Gallery = gal;
					model.Images = (from g in gal.GalleryImages
									where g.GalleryID == model.GalleryID
									orderby g.ImageOrder ascending
									select g).ToList();
				} else {
					model.Gallery = new GalleryGroup();
					model.Images = new List<GalleryImageEntry>();
				}
			}

			if (String.IsNullOrEmpty(settings.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(settings.AlternateViewFile, model);
			}
		}
	}
}