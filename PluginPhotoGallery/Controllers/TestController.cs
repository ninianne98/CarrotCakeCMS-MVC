using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CarrotCake.CMS.Plugins.PhotoGallery.Controllers {

	public class TestController : Controller {

		// GET: Test
		public ActionResult Index() {
			PagedData<tblGalleryImage> model = new PagedData<tblGalleryImage>();
			//model.PageSize = 10;
			//model.PageNumber = 1;
			model.InitOrderBy(x => x.ImageOrder, true);

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				model.DataSource = (from c in db.tblGalleryImages
									orderby c.ImageOrder ascending
									select c).Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

				model.TotalRecords = (from c in db.tblGalleryImages select c).Count();
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult Index(PagedData<tblGalleryImage> model) {
			model.ToggleSort();
			var srt = model.ParseSort();

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				IQueryable<tblGalleryImage> query = (from c in db.tblGalleryImages select c);

				query = query.SortByParm<tblGalleryImage>(srt.SortField, srt.SortDirection);

				model.DataSource = query.Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

				model.TotalRecords = (from c in db.tblGalleryImages select c).Count();
			}

			ModelState.Clear();

			return View(model);
		}

		public ActionResult Index2() {
			List<tblGalleryImage> lst = new List<tblGalleryImage>();

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				lst = (from c in db.tblGalleryImages
					   select c).ToList();
			}

			return View(lst);
		}
	}
}