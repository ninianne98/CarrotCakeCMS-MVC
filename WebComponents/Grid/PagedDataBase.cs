using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public abstract class PagedDataBase {

		public PagedDataBase() {
			this.PageSize = 10;
			this.MaxPage = 0;
			this.TotalRecords = -1;
			this.PageNumber = 1;
			this.PageNumbParm = "PageNbr";
			this.OrderBy = "ID  DESC";
		}

		public virtual void ReadPageNbr() {
			HttpContext context = HttpContext.Current;
			string sPageNbr = "";

			if (context.Request[this.PageNumbParm] != null) {
				sPageNbr = context.Request[this.PageNumbParm].ToString();
			}

			if (!String.IsNullOrEmpty(sPageNbr)) {
				int pg = int.Parse(sPageNbr);
				this.PageNumber = pg;
			}
		}

		public string OrderBy { get; set; }

		public string SortByNew { get; set; }

		public string PageNumbParm { get; set; }

		public int PageSize { get; set; }

		public int TotalRecords { get; set; }

		public int PageNumber { get; set; }

		public int PageNumberZeroIndex {
			get {
				return this.PageNumber - 1;
			}
		}

		public int MaxPage { get; set; }

		public int TotalPages {
			get {
				int iTotalPages = this.TotalRecords / this.PageSize;

				if ((this.TotalRecords % this.PageSize) > 0) {
					iTotalPages++;
				}

				if (this.MaxPage > 0 && this.MaxPage < iTotalPages) {
					iTotalPages = this.MaxPage;
				}

				return iTotalPages;
			}
		}

		public List<int> PageNumbers {
			get {
				List<int> pagelist = new List<int>();
				if (this.TotalPages > 1) {
					pagelist = Enumerable.Range(1, this.TotalPages).ToList();
				}
				return pagelist;
			}
		}

		public bool DefaultMax {
			get {
				return (this.PageNumber > this.TotalPages);
			}
		}

		public SortParm ParseSort() {
			SortParm sort = new SortParm(this.OrderBy);

			if (this.DefaultMax && this.TotalPages > 0) {
				this.PageNumber = this.TotalPages;
			}

			return sort;
		}

		public void ToggleSort() {
			SortParm srt = this.ParseSort();

			if (!String.IsNullOrEmpty(this.SortByNew)) {
				if (srt.SortField.ToLower() == this.SortByNew.ToLower()) {
					if (srt.SortDirection.EndsWith("ASC")) {
						this.OrderBy = String.Format("{0}  DESC", this.SortByNew);
					} else {
						this.OrderBy = String.Format("{0}  ASC", this.SortByNew);
					}
				} else {
					this.OrderBy = String.Format("{0}  ASC", this.SortByNew);
					this.PageNumber = 1;
				}
			}

			this.SortByNew = String.Empty;
		}

		public virtual bool HasData {
			get {
				return false;
			}
		}
	}

	//===================
	public class SortParm {

		public SortParm() { }

		public SortParm(string orderBy) {
			this.OrderBy = orderBy;
			Parse();
		}

		public void Parse() {
			string sortFld = String.Empty;
			string sortDir = String.Empty;

			if (!String.IsNullOrEmpty(this.OrderBy)) {
				int pos = this.OrderBy.LastIndexOf(" ");
				sortFld = this.OrderBy.Substring(0, pos).Trim();
				sortDir = this.OrderBy.Substring(pos).Trim();
			}

			this.SortField = sortFld;
			this.SortDirection = sortDir.ToUpper();
		}

		public string OrderBy { get; set; }

		public string SortField { get; set; }
		public string SortDirection { get; set; }
	}
}