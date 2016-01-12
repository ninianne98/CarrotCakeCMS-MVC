using System;

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

	public class BlogDatePathParser {
		private string _filename = String.Empty;
		private SiteData _site = new SiteData();

		private DateTime _dateBegin = DateTime.MinValue;
		private DateTime _dateEnd = DateTime.MaxValue;

		public int? Month { get; set; }
		public int? Day { get; set; }
		public int? Year { get; set; }

		public DateTime DateBegin { get { return _dateBegin; } }
		public DateTime DateEnd { get { return _dateEnd; } }

		public DateTime DateBeginUTC {
			get {
				if (_site != null) {
					return _site.ConvertSiteTimeToUTC(_dateBegin);
				} else {
					return _dateBegin;
				}
			}
		}

		public DateTime DateEndUTC {
			get {
				if (_site != null) {
					return _site.ConvertSiteTimeToUTC(_dateEnd);
				} else {
					return _dateEnd;
				}
			}
		}

		public BlogDatePathParser() {
			_filename = SiteData.CurrentScriptName;
			_site = SiteData.CurrentSite;

			ParseString();
		}

		public BlogDatePathParser(SiteData site) {
			_filename = SiteData.CurrentScriptName;
			_site = site;

			ParseString();
		}

		public BlogDatePathParser(string folderPath) {
			_filename = folderPath;
			_site = SiteData.CurrentSite;

			ParseString();
		}

		public BlogDatePathParser(SiteData site, string folderPath) {
			_filename = folderPath;
			_site = site;

			ParseString();
		}

		private void ParseString() {
			_filename = _filename.Replace(@"\", "/").Replace("//", "/").Replace("//", "/");
			string sFile = _filename.ToLowerInvariant().Replace(_site.BlogDateFolderPath, String.Empty);

			if (sFile.EndsWith(SiteData.SiteSearchPageName) ||
					sFile.EndsWith(String.Format("{0}.aspx", SiteData.SiteSearchPageName))) {
				sFile = sFile.Substring(0, sFile.ToLowerInvariant().LastIndexOf("/"));
			}

			string[] parms = sFile.Split('/');
			if (parms.Length > 2 && !parms[2].StartsWith(SiteData.SiteSearchPageName)) {
				this.Day = int.Parse(parms[2]);
			}
			if (parms.Length > 1 && !parms[1].StartsWith(SiteData.SiteSearchPageName)) {
				this.Month = int.Parse(parms[1]);
			}
			if (parms.Length > 0 && !parms[0].StartsWith(SiteData.SiteSearchPageName)) {
				this.Year = int.Parse(parms[0]);
			}

			if (this.Month == null && this.Day == null) {
				_dateBegin = new DateTime(Convert.ToInt32(this.Year), 1, 1);
				_dateEnd = _dateBegin.AddYears(1).AddMilliseconds(-1);
			}
			if (this.Month != null && this.Day == null) {
				_dateBegin = new DateTime(Convert.ToInt32(this.Year), Convert.ToInt32(this.Month), 1);
				_dateEnd = _dateBegin.AddMonths(1).AddMilliseconds(-1);
			}
			if (this.Month != null && this.Day != null) {
				_dateBegin = new DateTime(Convert.ToInt32(this.Year), Convert.ToInt32(this.Month), Convert.ToInt32(this.Day));
				_dateEnd = _dateBegin.AddDays(1).AddMilliseconds(-1);
			}
		}
	}
}