using CarrotCake.CMS.Plugins.EventCalendarModule.Code;
using CarrotCake.CMS.Plugins.EventCalendarModule.Models;
using Carrotware.CMS.Core;
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

namespace CarrotCake.CMS.Plugins.EventCalendarModule.Controllers {

	public class AdminController : BaseAdminWidgetController {
		private CalendarDataContext _db = CalendarDataContext.GetDataContext();

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (_db != null) {
				_db.Dispose();
			}
		}

		protected override void Initialize(RequestContext requestContext) {
			base.Initialize(requestContext);

			if (this.TestSiteID != Guid.Empty.ToString()) {
				this.SiteID = new Guid(this.TestSiteID);
			}
			if (SiteData.CurrentSiteExists) {
				this.SiteID = SiteData.CurrentSiteID;
			}
		}

		public ActionResult Index() {
			return View();
		}

		public ActionResult Database() {
			List<string> lst = new List<string>();

			DatabaseUpdate du = new DatabaseUpdate();
			DatabaseUpdateResponse dbRes = new DatabaseUpdateResponse();

			string sqlUpdate = string.Empty;
			string sqlTest = string.Empty;

			try {
				sqlUpdate = WebHelper.ReadEmbededScript("CarrotCake.CMS.Plugins.EventCalendarModule.carrot_CalendarEvent.sql");

				sqlTest = "select * from [INFORMATION_SCHEMA].[COLUMNS] where table_name in('carrot_CalendarEvent','carrot_CalendarEventCategory')";
				dbRes = du.ApplyUpdateIfNotFound(sqlTest, sqlUpdate, false);

				if (dbRes.LastException != null && !string.IsNullOrEmpty(dbRes.LastException.Message)) {
					lst.Add(dbRes.LastException.Message);
				} else {
					lst.Add(dbRes.Response);
				}
			} catch (Exception ex) {
				lst.Add(ex.ToString());
			}

			CalendarHelper.SeedCalendarCategories(this.SiteID);

			return View(lst);
		}

		public ActionResult CategoryList() {
			CalendarHelper.SeedCalendarCategories(this.SiteID);

			var model = new PagedData<carrot_CalendarEventCategory>();
			model.InitOrderBy(x => x.CategoryName);

			return CategoryList(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CategoryList(PagedData<carrot_CalendarEventCategory> model) {
			model.ToggleSort();
			var srt = model.ParseSort();

			var query = CalendarHelper.GetCalendarCategories(this.SiteID).SortByParm(srt.SortField, srt.SortDirection);
			model.DataSource = query.ToList();
			model.TotalRecords = query.Count();

			ModelState.Clear();
			return View(model);
		}

		public ActionResult CategoryDetail(Guid? id) {
			var itemId = id.HasValue ? id.Value : Guid.Empty;
			var model = new carrot_CalendarEventCategory();

			if (itemId == Guid.Empty) {
				model.CalendarEventCategoryID = itemId;
				model.CategoryBGColor = CalendarHelper.HEX_White;
				model.CategoryFGColor = CalendarHelper.HEX_Black;
			} else {
				model = CalendarHelper.GetCalendarCategory(itemId);
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CategoryDetail(carrot_CalendarEventCategory model) {
			if (ModelState.IsValid) {
				bool bAdd = false;

				var itm = (from c in _db.carrot_CalendarEventCategories
						   where c.CalendarEventCategoryID == model.CalendarEventCategoryID
						   select c).FirstOrDefault();

				if (itm == null) {
					bAdd = true;
					itm = new carrot_CalendarEventCategory();
					itm.CalendarEventCategoryID = Guid.NewGuid();
					itm.SiteID = this.SiteID;

					model.CalendarEventCategoryID = itm.CalendarEventCategoryID;
				}

				itm.CategoryName = model.CategoryName;
				itm.CategoryFGColor = model.CategoryFGColor;
				itm.CategoryBGColor = model.CategoryBGColor;

				if (bAdd) {
					_db.carrot_CalendarEventCategories.InsertOnSubmit(itm);
				}

				_db.SubmitChanges();

				return RedirectToAction(this.GetActionName(x => x.CategoryDetail(model.CalendarEventCategoryID)), new { @id = model.CalendarEventCategoryID });
			}

			return View(model);
		}

		public ActionResult ProfileList() {
			CalendarHelper.SeedCalendarCategories(this.SiteID);

			var model = new ProfileDisplayModel(this.SiteID);
			model.Load();

			return View(model);
		}

		[HttpPost]
		public ActionResult ProfileList(ProfileDisplayModel model) {
			model.SiteID = this.SiteID;
			model.Load();

			ModelState.Clear();
			return View(model);
		}

		public ActionResult EventDetail(Guid? id) {
			CalendarHelper.SeedCalendarCategories(this.SiteID);

			var itemId = id.HasValue ? id.Value : Guid.Empty;
			var model = new EventDetailModel(this.SiteID, itemId);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult EventDetail(EventDetailModel model) {
			this.TryValidateModel(model);
			model.SiteID = this.SiteID;

			if (model.Operation.ToLowerInvariant() == "delete") {
				CalendarHelper.RemoveEvent(model.ItemID);
				return RedirectToAction(this.GetActionName(x => x.ProfileList()));
			}

			if (ModelState.IsValid) {
				bool bAdd = false;
				if (model.Operation.ToLowerInvariant() == "copy") {
					var p = CalendarHelper.CopyEvent(model.ItemID);
					model.ItemID = p.CalendarEventProfileID;
					model.ItemData.CalendarEventProfileID = p.CalendarEventProfileID;
				}

				var currItem = (from c in _db.carrot_CalendarEventProfiles
								where c.CalendarEventProfileID == model.ItemID
								select c).FirstOrDefault();

				var origItem = new CalendarEvent(currItem);

				if (currItem == null) {
					bAdd = true;
					model.ItemID = Guid.NewGuid();
					currItem = new carrot_CalendarEventProfile();
					currItem.CalendarEventProfileID = model.ItemID;
					currItem.SiteID = this.SiteID;
					currItem.IsHoliday = false;
					currItem.IsAnnualHoliday = false;
					currItem.RecursEvery = 1;
				}

				currItem.CalendarFrequencyID = model.ItemData.CalendarFrequencyID;
				currItem.CalendarEventCategoryID = model.ItemData.CalendarEventCategoryID;

				currItem.EventRepeatPattern = null;

				List<int> days = model.DaysOfTheWeek.Where(x => x.Selected).Select(x => int.Parse(x.Value)).ToList();

				if (CalendarFrequencyHelper.GetFrequencyTypeByID(currItem.CalendarFrequencyID) == CalendarFrequencyHelper.FrequencyType.Weekly
							&& days.Count > 0) {
					int dayMask = (from d in days select d).Sum();

					if (dayMask > 0) {
						currItem.EventRepeatPattern = dayMask;
					}
				}

				currItem.EventTitle = model.ItemData.EventTitle;
				currItem.EventDetail = model.ItemData.EventDetail;
				currItem.RecursEvery = model.ItemData.RecursEvery;

				currItem.IsPublic = model.ItemData.IsPublic;
				currItem.IsAllDayEvent = model.ItemData.IsAllDayEvent;
				currItem.IsCancelled = model.ItemData.IsCancelled;
				currItem.IsCancelledPublic = model.ItemData.IsCancelledPublic;

				currItem.EventStartDate = model.ItemData.EventStartDate.Date;
				currItem.EventStartTime = currItem.IsAllDayEvent ? null : CalendarHelper.GetTimeSpan(model.EventStartTime);

				currItem.EventEndDate = model.ItemData.EventEndDate.Date;
				currItem.EventEndTime = currItem.IsAllDayEvent ? null : CalendarHelper.GetTimeSpan(model.EventEndTime);

				if ((currItem.EventEndDate + currItem.EventEndTime) < (currItem.EventStartDate + currItem.EventStartTime)) {
					currItem.EventEndDate = currItem.EventStartDate;
					currItem.EventEndTime = currItem.EventStartTime;
				}

				if (bAdd) {
					_db.carrot_CalendarEventProfiles.InsertOnSubmit(currItem);
				}

				CalendarFrequencyHelper.SaveFrequencies(_db, new CalendarEvent(currItem), origItem);

				_db.SubmitChanges();

				return RedirectToAction(this.GetActionName(x => x.EventDetail(model.ItemID)), new { @id = model.ItemID });
			}

			model.Load();

			return View(model);
		}

		public ActionResult EventList() {
			CalendarHelper.SeedCalendarCategories(this.SiteID);

			CalendarViewModel model = new CalendarViewModel();

			return EventList(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EventList(CalendarViewModel model) {
			if (ModelState.IsValid) {
				ModelState.Clear();

				model.LoadData(this.SiteID, false);
			}

			return View(model);
		}

		public ActionResult EventDetailSingle(Guid? id) {
			var itemId = id.HasValue ? id.Value : Guid.Empty;
			var model = new EventSingleModel(this.SiteID, itemId);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult EventDetailSingle(EventSingleModel model) {
			this.TryValidateModel(model);
			model.SiteID = this.SiteID;

			if (ModelState.IsValid) {
				bool bAdd = false;

				var currItem = (from c in _db.carrot_CalendarEvents
								where c.CalendarEventID == model.ItemID
								select c).FirstOrDefault();

				if (currItem == null) {
					bAdd = true;
					model.ItemID = Guid.NewGuid();
					currItem = new carrot_CalendarEvent();
					currItem.CalendarEventID = model.ItemID;
				}

				currItem.EventDetail = model.ItemData.EventDetail;
				currItem.IsCancelled = model.ItemData.IsCancelled;

				currItem.EventStartTime = CalendarHelper.GetTimeSpan(model.EventStartTime);
				currItem.EventEndTime = CalendarHelper.GetTimeSpan(model.EventEndTime);

				if (bAdd) {
					_db.carrot_CalendarEvents.InsertOnSubmit(currItem);
				}

				_db.SubmitChanges();

				return RedirectToAction(this.GetActionName(x => x.EventDetailSingle(model.ItemID)), new { @id = model.ItemID });
			}

			model.Load();

			return View(model);
		}
	}
}