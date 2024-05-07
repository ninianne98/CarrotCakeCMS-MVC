using Carrotware.CMS.Data;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class EditHistory {

		public EditHistory() { }

		public EditHistory(vw_carrot_EditHistory p) {
			if (p != null) {
				SiteData site = SiteData.GetSiteFromCache(p.SiteID);

				this.SiteID = p.SiteID;
				this.ContentID = p.ContentID;
				this.Root_ContentID = p.Root_ContentID;
				this.IsLatestVersion = p.IsLatestVersion;
				this.TitleBar = p.TitleBar;
				this.NavMenuText = p.NavMenuText;
				this.PageHead = p.PageHead;
				this.EditUserId = p.EditUserId;
				this.EditDate = site.ConvertUTCToSiteTime(p.EditDate);
				this.FileName = p.FileName;
				this.ContentTypeID = p.ContentTypeID;
				this.ContentTypeValue = p.ContentTypeValue;
				this.PageActive = p.PageActive;
				this.GoLiveDate = site.ConvertUTCToSiteTime(p.GoLiveDate);
				this.RetireDate = site.ConvertUTCToSiteTime(p.RetireDate);
				this.CreateDate = site.ConvertUTCToSiteTime(p.CreateDate);
				this.EditUserName = p.EditUserName;
				this.EditEmail = p.EditEmail;
				this.CreateUserName = p.CreateUserName;
				this.CreateEmail = p.CreateEmail;
			}
		}

		public Guid SiteID { get; set; }
		public Guid ContentID { get; set; }
		public Guid Root_ContentID { get; set; }

		[Display(Name = "Is Latest")]
		public bool IsLatestVersion { get; set; }

		[Display(Name = "Title")]
		public string TitleBar { get; set; }

		[Display(Name = "Nav Text")]
		public string NavMenuText { get; set; }

		[Display(Name = "Heading")]
		public string PageHead { get; set; }

		public Guid? EditUserId { get; set; }

		[Display(Name = "Edited Date")]
		public DateTime EditDate { get; set; }

		[Display(Name = "File Name")]
		public string FileName { get; set; }

		public Guid ContentTypeID { get; set; }

		[Display(Name = "Content Type")]
		public string ContentTypeValue { get; set; }

		[Display(Name = "Public")]
		public bool PageActive { get; set; }

		[Display(Name = "Go Live Date")]
		public DateTime GoLiveDate { get; set; }

		[Display(Name = "Retire Date")]
		public DateTime RetireDate { get; set; }

		[Display(Name = "Edited By")]
		public string EditUserName { get; set; }

		[Display(Name = "Editor Email")]
		public string EditEmail { get; set; }

		[Display(Name = "Created Date")]
		public DateTime CreateDate { get; set; }

		[Display(Name = "Created By")]
		public string CreateUserName { get; set; }

		[Display(Name = "Creator Email")]
		public string CreateEmail { get; set; }

		public static int GetHistoryListCount(Guid siteID, bool showLatestOnly, DateTime? editDate, Guid? editUserID) {
			Guid userID = Guid.Empty;
			if (editUserID.HasValue) {
				userID = editUserID.Value;
			}

			DateTime dateStart = DateTime.UtcNow.Date.AddDays(-2);
			DateTime dateEnd = DateTime.UtcNow.Date.AddDays(1);

			if (editDate.HasValue) {
				dateStart = editDate.Value.Date.AddDays(-8);
				dateEnd = editDate.Value.Date.AddDays(1);
			}

			using (var db = CarrotCMSDataContext.Create()) {
				return (from h in db.vw_carrot_EditHistories
						where h.SiteID == siteID
							&& (!showLatestOnly || h.IsLatestVersion == true)
							&& (!editDate.HasValue
								  || (h.EditDate.Date >= dateStart.Date && h.EditDate.Date <= dateEnd.Date))
							&& (!editUserID.HasValue || h.EditUserId == userID)
						select h).Count();
			}
		}

		public static List<EditHistory> GetHistoryList(string orderBy, int pageNumber, int pageSize,
				Guid siteID, bool showLatestOnly, DateTime? editDate, Guid? editUserID) {
			SortParm srt = new SortParm(orderBy);

			if (String.IsNullOrEmpty(srt.SortField)) {
				srt.SortField = "EditDate";
			}

			if (String.IsNullOrEmpty(srt.SortDirection)) {
				srt.SortDirection = "DESC";
			}

			Guid userID = Guid.Empty;
			if (editUserID.HasValue) {
				userID = editUserID.Value;
			}

			DateTime dateStart = DateTime.UtcNow.Date.AddDays(-2);
			DateTime dateEnd = DateTime.UtcNow.Date.AddDays(1);

			if (editDate.HasValue) {
				dateStart = editDate.Value.Date.AddDays(-8);
				dateEnd = editDate.Value.Date.AddDays(1);
			}

			int startRec = pageNumber * pageSize;

			if (pageSize < 0 || pageSize > 200) {
				pageSize = 25;
			}

			if (pageNumber < 0 || pageNumber > 10000) {
				pageNumber = 0;
			}

			bool IsContentProp = false;

			srt.SortField = (from p in ReflectionUtilities.GetPropertyStrings(typeof(vw_carrot_EditHistory))
							 where p.ToLowerInvariant().Trim() == srt.SortField.ToLowerInvariant().Trim()
							 select p).FirstOrDefault();

			if (!String.IsNullOrEmpty(srt.SortField)) {
				IsContentProp = ReflectionUtilities.DoesPropertyExist(typeof(vw_carrot_EditHistory), srt.SortField);
			}

			using (var db = CarrotCMSDataContext.Create()) {
				List<EditHistory> _history = null;

				IQueryable<vw_carrot_EditHistory> queryable = (from h in db.vw_carrot_EditHistories
															   where h.SiteID == siteID
																   && (!showLatestOnly || h.IsLatestVersion == true)
																   && (!editDate.HasValue
																		 || (h.EditDate.Date >= dateStart.Date && h.EditDate.Date <= dateEnd.Date))
																   && (!editUserID.HasValue || h.EditUserId == userID)
															   select h);

				if (IsContentProp) {
					queryable = queryable.SortByParm(srt.SortField, srt.SortDirection);
				} else {
					queryable = (from c in queryable
								 orderby c.EditDate descending
								 where c.SiteID == siteID
								 select c).AsQueryable();
				}

				_history = (from h in queryable.PaginateListFromZero(pageNumber, pageSize) select new EditHistory(h)).ToList();

				return _history;
			}
		}
	}
}