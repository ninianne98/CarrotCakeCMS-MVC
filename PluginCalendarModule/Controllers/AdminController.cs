﻿using CarrotCake.CMS.Plugins.CalendarModule.Code;
using CarrotCake.CMS.Plugins.CalendarModule.Models;
using Carrotware.CMS.DBUpdater;
using Carrotware.CMS.Interface.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace CarrotCake.CMS.Plugins.CalendarModule.Controllers {

	public class AdminController : BaseAdminWidgetController {
		private dbCalendarDataContext db = dbCalendarDataContext.GetDataContext();

		[HttpGet]
		public ActionResult Index() {
			CalendarViewModel model = new CalendarViewModel();

			return Index(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Index(CalendarViewModel model) {
			if (ModelState.IsValid) {
				ModelState.Clear();

				model.LoadData(this.SiteID, false);
			}

			return View(model);
		}

		public ActionResult CalendarDatabase() {
			List<string> lst = new List<string>();

			DatabaseUpdate du = new DatabaseUpdate();
			DatabaseUpdateResponse dbRes = new DatabaseUpdateResponse();

			string sqlUpdate = String.Empty;
			string sqlTest = String.Empty;

			try {
				sqlUpdate = WebHelper.ReadEmbededScript("CarrotCake.CMS.Plugins.CalendarModule.tblCalendar.sql");

				sqlTest = "select * from [INFORMATION_SCHEMA].[COLUMNS] where table_name in('tblCalendar')";
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

		public ActionResult CalendarAdminAdd(Guid? id) {
			return CalendarAdminAddEdit(Guid.Empty);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult CalendarAdminAdd(CalendarDetail model) {
			return CalendarAdminAddEdit(model);
		}

		public ActionResult CalendarAdminAddEdit(Guid? id) {
			Guid ItemGuid = id ?? Guid.Empty;

			CalendarDetail model = (from c in db.tblCalendars
									where c.CalendarID == ItemGuid
										 && SiteID == this.SiteID
									select new CalendarDetail(c)).FirstOrDefault();

			if (model == null) {
				model = new CalendarDetail {
					SiteID = this.SiteID,
					CalendarID = Guid.Empty,
					IsActive = true,
					EventDate = DateTime.Now.Date
				};
			}

			return View("CalendarAdminAddEdit", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult CalendarAdminAddEdit(CalendarDetail model) {
			if (ModelState.IsValid) {
				ModelState.Clear();

				var itm = (from c in db.tblCalendars
						   where c.CalendarID == model.CalendarID
								&& SiteID == this.SiteID
						   select c).FirstOrDefault();

				if (itm == null || model.CalendarID == Guid.Empty) {
					itm = new tblCalendar {
						SiteID = this.SiteID,
						CalendarID = Guid.NewGuid(),
						EventDate = DateTime.Now.Date
					};

					db.tblCalendars.InsertOnSubmit(itm);
				}

				itm.EventDate = model.EventDate;
				itm.EventTitle = model.EventTitle;
				itm.EventDetail = model.EventDetail;
				itm.IsActive = model.IsActive;

				db.SubmitChanges();

				return RedirectToAction("CalendarAdminAddEdit", new { @id = itm.CalendarID });
			}

			return View("CalendarAdminAddEdit", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult CalendarAdminDelete(CalendarDetail model) {
			var itm = (from c in db.tblCalendars
					   where c.CalendarID == model.CalendarID
							&& SiteID == this.SiteID
					   select c).FirstOrDefault();

			db.tblCalendars.DeleteOnSubmit(itm);
			db.SubmitChanges();

			return RedirectToAction("Index");
		}

		public ActionResult CalendarAdminCat() {
			return View();
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (db != null) {
				db.Dispose();
			}
		}
	}
}