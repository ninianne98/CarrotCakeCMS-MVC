using System;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class BlogDatePathParser {
		private string _FileName = String.Empty;
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
			_FileName = SiteData.CurrentScriptName;
			_site = SiteData.CurrentSite;

			ParseString();
		}

		public BlogDatePathParser(SiteData site) {
			_FileName = SiteData.CurrentScriptName;
			_site = site;

			ParseString();
		}

		public BlogDatePathParser(string FolderPath) {
			_FileName = FolderPath;
			_site = SiteData.CurrentSite;

			ParseString();
		}

		public BlogDatePathParser(SiteData site, string FolderPath) {
			_FileName = FolderPath;
			_site = site;

			ParseString();
		}

		private void ParseString() {
			_FileName = _FileName.Replace(@"\", "/").Replace("//", "/").Replace("//", "/");
			string sFile = _FileName.ToLower().Replace(_site.BlogDateFolderPath, "");

			if (sFile.ToLower().EndsWith(SiteData.SiteSearchPageName)) {
				sFile = sFile.ToLower().Substring(0, sFile.ToLower().LastIndexOf("/"));
			}

			string[] parms = sFile.Split('/');
			if (parms.Length > 2) {
				Day = int.Parse(parms[2]);
			}
			if (parms.Length > 1) {
				Month = int.Parse(parms[1]);
			}
			if (parms.Length > 0) {
				Year = int.Parse(parms[0]);
			}

			if (Month == null && Day == null) {
				_dateBegin = new DateTime(Convert.ToInt32(this.Year), 1, 1);
				_dateEnd = _dateBegin.AddYears(1).AddMilliseconds(-1);
			}
			if (Month != null && Day == null) {
				_dateBegin = new DateTime(Convert.ToInt32(this.Year), Convert.ToInt32(this.Month), 1);
				_dateEnd = _dateBegin.AddMonths(1).AddMilliseconds(-1);
			}
			if (Month != null && Day != null) {
				_dateBegin = new DateTime(Convert.ToInt32(this.Year), Convert.ToInt32(this.Month), Convert.ToInt32(this.Day));
				_dateEnd = _dateBegin.AddDays(1).AddMilliseconds(-1);
			}
		}
	}
}