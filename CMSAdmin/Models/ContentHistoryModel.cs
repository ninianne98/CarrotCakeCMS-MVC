using Carrotware.CMS.Core;
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
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class ContentHistoryModel {

		public ContentHistoryModel() {
			this.GetLatestOnly = true;
			this.SearchDate = DateTime.Now.Date;
			this.Page = new PagedData<EditHistory>();

			this.PageSizes = new List<int>();
			this.PageSizes.Add(10);
			this.PageSizes.Add(25);
			this.PageSizes.Add(50);
			this.PageSizes.Add(100);
		}

		[Display(Name = "Search Date")]
		public DateTime? SearchDate { get; set; }

		[Display(Name = "Show Latest Only")]
		public bool GetLatestOnly { get; set; }

		public List<int> PageSizes { get; set; }

		public PagedData<EditHistory> Page { get; set; }

		[Display(Name = "Filer by Editor")]
		public Guid? SelectedUserID { get; set; }

		private List<ExtendedUserData> _users = null;

		public Dictionary<Guid, string> UserList {
			get {
				if (_users == null) {
					_users = ExtendedUserData.GetUserList();
				}

				return (from p in _users
						orderby p.UserName
						select p).ToDictionary(kvp => kvp.UserId, kvp => kvp.UserName);
			}
		}
	}
}