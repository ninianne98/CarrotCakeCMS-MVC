using CarrotCake.CMS.Plugins.PhotoGallery.Models;
using Carrotware.CMS.DBUpdater;
using Carrotware.CMS.Interface.Controllers;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace CarrotCake.CMS.Plugins.PhotoGallery.Controllers {

	public class AdminController : BaseAdminWidgetController {
		private GalleryHelper _helper;

		protected override void Initialize(RequestContext requestContext) {
			base.Initialize(requestContext);

			if (this.TestSiteID != Guid.Empty.ToString()) {
				this.SiteID = new Guid(this.TestSiteID);
			}

			_helper = new GalleryHelper(this.SiteID);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (_helper != null) {
				_helper.Dispose();
			}
		}

		public ActionResult Index() {
			var model = new PagedData<GalleryGroup>();
			model.InitOrderBy(x => x.GalleryTitle, true);
			model.PageSize = 25;
			model.PageNumber = 1;

			return Index(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Index(PagedData<GalleryGroup> model) {
			model.ToggleSort();
			var srt = model.ParseSort();

			model.TotalRecords = _helper.GalleryGroupListGetBySiteIDCount();
			var query = _helper.GalleryGroupListGetBySiteID();
			query = query.SortByParm(srt.SortField, srt.SortDirection);

			model.DataSource = query.Skip(model.PageSize * model.PageNumberZeroIndex).Take(model.PageSize)
						.Select(x => new GalleryGroup(x)).ToList();

			ModelState.Clear();

			return View(model);
		}

		public ActionResult EditGallery(Guid id) {
			return View("EditGallery", _helper.GalleryGroupGetByID(id));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditGallery(GalleryGroup model) {
			if (ModelState.IsValid) {
				GalleryGroup m = _helper.GalleryGroupGetByID(model.GalleryID);
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

			if (!model.SaveGallery) {
				model.LoadGallery();

				return View(model);
			} else {
				model.Save();
				model.LoadGallery();

				return RedirectToAction("EditGalleryPhotos", new { @id = model.GalleryID });
			}
		}

		public ActionResult GalleryDatabase() {
			var lst = new List<string>();
			var du = new DatabaseUpdate();

			try {
				var sqlUpdate = GalleryHelper.ReadEmbededScript("CarrotCake.CMS.Plugins.PhotoGallery.tblGallery.sql");

				var sqlTest = "select * from [INFORMATION_SCHEMA].[COLUMNS] where table_name in('tblGalleryImageMeta')";

				var res = du.ApplyUpdateIfNotFound(sqlTest, sqlUpdate, false);

				if (res.LastException != null && !string.IsNullOrEmpty(res.LastException.Message)) {
					lst.Add(res.LastException.Message);
				} else {
					lst.Add(res.Response);
				}
			} catch (Exception ex) {
				lst.Add(ex.ToString());
			}

			return View(lst);
		}

		[HttpGet]
		public ActionResult EditImageMetaData(string path) {
			string imageFile = string.Empty;

			if (!string.IsNullOrEmpty(path)) {
				imageFile = path.DecodeBase64();
			}

			ValidateGalleryImage(imageFile);

			GalleryMetaData model = _helper.GalleryMetaDataGetByFilename(imageFile);
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
			GalleryMetaData meta = _helper.GalleryMetaDataGetByFilename(model.GalleryImage);

			if (meta == null) {
				meta = new GalleryMetaData();
				meta.GalleryImageMetaID = Guid.Empty;
				meta.SiteID = this.SiteID;
				meta.GalleryImage = model.GalleryImage.ToLower();
			}

			meta.ValidateGalleryImage();

			meta.ImageMetaData = model.ImageMetaData;
			meta.ImageTitle = model.ImageTitle;
			meta.Save();

			return RedirectToAction("EditImageMetaData", new { @path = meta.GalleryImage.EncodeBase64() });
		}

		protected void ValidateGalleryImage(string imageFile) {
			if (imageFile.Contains("../") || imageFile.Contains(@"..\")) {
				throw new Exception("Cannot use relative paths.");
			}
			if (imageFile.Contains(":")) {
				throw new Exception("Cannot specify drive letters.");
			}
			if (imageFile.Contains("//") || imageFile.Contains(@"\\")) {
				throw new Exception("Cannot use UNC paths.");
			}
			if (imageFile.Contains("<") || imageFile.Contains(">")) {
				throw new Exception("Cannot include html tags.");
			}
		}
	}
}