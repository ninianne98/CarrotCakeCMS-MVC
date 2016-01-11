using CarrotCake.CMS.Plugins.PhotoGallery.Models;
using Carrotware.CMS.DBUpdater;
using Carrotware.CMS.Interface.Controllers;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CarrotCake.CMS.Plugins.PhotoGallery.Controllers {

	public class AdminController : BaseAdminWidgetController {

		public ActionResult Index() {
			PagedData<GalleryGroup> model = new PagedData<GalleryGroup>();
			model.InitOrderBy(x => x.GalleryTitle, true);
			model.PageSize = 25;
			model.PageNumber = 1;

			return Index(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Index(PagedData<GalleryGroup> model) {
			GalleryHelper gh = new GalleryHelper(this.SiteID);

			model.ToggleSort();
			var srt = model.ParseSort();

			List<GalleryGroup> lst = gh.GalleryGroupListGetBySiteID();

			IQueryable<GalleryGroup> query = lst.AsQueryable();
			query = query.SortByParm<GalleryGroup>(srt.SortField, srt.SortDirection);

			model.DataSource = query.Skip(model.PageSize * model.PageNumberZeroIndex).Take(model.PageSize).ToList();

			model.TotalRecords = lst.Count();

			ModelState.Clear();

			return View(model);
		}

		public ActionResult EditGallery(Guid id) {
			GalleryHelper gh = new GalleryHelper(this.SiteID);

			return View("EditGallery", gh.GalleryGroupGetByID(id));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditGallery(GalleryGroup model) {
			if (ModelState.IsValid) {
				GalleryHelper gh = new GalleryHelper(this.SiteID);
				GalleryGroup m = gh.GalleryGroupGetByID(model.GalleryID);
				if (m == null) {
					m = new GalleryGroup();
					m.SiteID = this.SiteID;
				}

				m.GalleryTitle = model.GalleryTitle;
				m.Save();

				return RedirectToAction("Index");
			} else {
				return View("EditGallery", model);
			}
		}

		public ActionResult CreateGallery() {
			return View("EditGallery", new GalleryGroup());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreateGallery(GalleryGroup model) {
			return EditGallery(model);
		}

		public ActionResult EditGalleryPhotos(Guid id) {
			EditPhotoGalleryModel model = new EditPhotoGalleryModel(this.SiteID, id);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditGalleryPhotos(EditPhotoGalleryModel model) {
			model.SetSrcFiles();
			model.LoadGallery();

			if (!model.SaveGallery) {
				return View(model);
			} else {
				model.Save();
				return RedirectToAction("EditGalleryPhotos", new { @id = model.GalleryID });
			}
		}

		public ActionResult GalleryDatabase() {
			List<string> lst = new List<string>();

			DatabaseUpdate du = new DatabaseUpdate();
			DatabaseUpdateResponse dbRes = new DatabaseUpdateResponse();
			string sqlUpdate = "";
			string sqlTest = "";
			try {
				sqlUpdate = GalleryHelper.ReadEmbededScript("CarrotCake.CMS.Plugins.PhotoGallery.tblGallery.sql");

				sqlTest = "select * from [INFORMATION_SCHEMA].[COLUMNS] where table_name in('tblGalleryImageMeta')";
				dbRes = du.ApplyUpdateIfNotFound(sqlTest, sqlUpdate, false);

				if (dbRes.LastException != null && !string.IsNullOrEmpty(dbRes.LastException.Message)) {
					lst.Add(dbRes.LastException.Message);
				} else {
					lst.Add(dbRes.Response);
				}
			} catch (Exception ex) {
				lst.Add(ex.ToString());
			}

			return View(lst);
		}

		public ActionResult EditImageMetaData(string path) {
			GalleryHelper gh = new GalleryHelper(this.SiteID);
			string imageFile = String.Empty;

			if (!String.IsNullOrEmpty(path)) {
				imageFile = Utils.DecodeBase64(path);
			}

			GalleryMetaData model = gh.GalleryMetaDataGetByFilename(imageFile);
			if (model == null) {
				model = new GalleryMetaData();
				model.SiteID = this.SiteID;
				model.GalleryImageMetaID = Guid.Empty;
				model.GalleryImage = imageFile;
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult EditImageMetaData(GalleryMetaData model) {
			GalleryHelper gh = new GalleryHelper(this.SiteID);

			GalleryMetaData meta = gh.GalleryMetaDataGetByFilename(model.GalleryImage);

			if (meta == null) {
				meta = new GalleryMetaData();
				meta.GalleryImageMetaID = Guid.Empty;
				meta.SiteID = this.SiteID;
				meta.GalleryImage = model.GalleryImage.ToLower();
			}

			meta.ImageMetaData = model.ImageMetaData;
			meta.ImageTitle = model.ImageTitle;
			meta.Save();

			return RedirectToAction("EditImageMetaData", new { @path = Utils.EncodeBase64(meta.GalleryImage) });
		}
	}
}