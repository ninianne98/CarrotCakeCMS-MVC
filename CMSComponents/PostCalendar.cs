using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Carrotware.CMS.UI.Components {

	public class PostCalendar : IWebComponent, IHtmlString {

		public PostCalendar() {
			this.ElementId = "cal";
		}

		public PostCalendar(string id) {
			this.ElementId = id;
		}

		protected ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper();

		public string CalendarHead { get; set; }

		public string CssClass { get; set; }

		public string ElementId { get; set; }

		public string CssClassCaption { get; set; }

		public string CssClassTable { get; set; }

		public string CssClassDayHead { get; set; }

		public string CssClassTableBody { get; set; }

		public string CssClassDateLink { get; set; }

		public string CssClassTableFoot { get; set; }

		private DateTime _date = DateTime.MinValue;

		private DateTime ThisMonth {
			get {
				if (_date.Year < 1900) {
					if (SiteData.IsWebView) {
						_date = SiteData.CurrentSite.Now.Date;
						string sFilterPath = SiteData.CurrentScriptName;
						if (SiteData.CurrentSite.CheckIsBlogDateFolderPath(sFilterPath)) {
							BlogDatePathParser p = new BlogDatePathParser(SiteData.CurrentSite, sFilterPath);
							if (p.DateBegin.Year > 1900) {
								_date = p.DateBegin;
							}
						}
					} else {
						_date = DateTime.Now;
					}
				}
				return _date;
			}
		}

		protected List<ContentDateLinks> GetDates() {
			if (SiteData.IsWebView) {
				return navHelper.GetSingleMonthBlogUpdateList(SiteData.CurrentSite, this.ThisMonth, !SecurityData.IsAuthEditor);
			} else {
				return new List<ContentDateLinks>();
			}
		}

		public string ToHtmlString() {
			var sb = new StringBuilder();

			List<ContentDateLinks> lstCalendar = GetDates();

			sb.AppendLine();

			string sCSS = string.Empty;
			if (!string.IsNullOrEmpty(this.CssClass)) {
				sCSS = " class=\"" + this.CssClass + "\" ";
			}

			string sCssClassTable = string.Empty;
			if (!string.IsNullOrEmpty(this.CssClassTable)) {
				sCssClassTable = " class=\"" + this.CssClassTable + "\" ";
			}
			string sCssClassCaption = string.Empty;
			if (!string.IsNullOrEmpty(this.CssClassCaption)) {
				sCssClassCaption = " class=\"" + this.CssClassCaption + "\" ";
			}
			string sCssClassDayHead = string.Empty;
			if (!string.IsNullOrEmpty(this.CssClassDayHead)) {
				sCssClassDayHead = " class=\"" + this.CssClassDayHead + "\" ";
			}
			string sCssClassTableBody = string.Empty;
			if (!string.IsNullOrEmpty(this.CssClassTableBody)) {
				sCssClassTableBody = " class=\"" + this.CssClassTableBody + "\" ";
			}
			string sCssClassDateLink = string.Empty;
			if (!string.IsNullOrEmpty(this.CssClassDateLink)) {
				sCssClassDateLink = " class=\"" + this.CssClassDateLink + "\" ";
			}
			string sCssClassTableFoot = string.Empty;
			if (!string.IsNullOrEmpty(this.CssClassTableFoot)) {
				sCssClassTableFoot = " class=\"" + this.CssClassTableFoot + "\" ";
			}

			ContentDateTally thisMonth = new ContentDateTally { TallyDate = this.ThisMonth, TheSite = SiteData.CurrentSite };
			ContentDateTally lastMonth = new ContentDateTally { TallyDate = this.ThisMonth.AddMonths(-1), TheSite = SiteData.CurrentSite };
			ContentDateTally nextMonth = new ContentDateTally { TallyDate = this.ThisMonth.AddMonths(1), TheSite = SiteData.CurrentSite };

			sb.AppendLine("<div" + sCSS + " id=\"" + this.ElementId + "\"> ");

			if (!string.IsNullOrEmpty(this.CalendarHead)) {
				sb.AppendLine("<h2 class=\"calendar-caption\">" + this.CalendarHead + "  </h2> ");
			}

			DateTime firstOfMonth = this.ThisMonth.AddDays(1 - this.ThisMonth.Day);
			int firstDay = (int)firstOfMonth.DayOfWeek;
			TimeSpan ts = firstOfMonth.AddMonths(1) - firstOfMonth;
			int daysInMonth = ts.Days;

			int yearNumber = firstOfMonth.Date.Year;
			int monthNumber = firstOfMonth.Date.Month;

			int weekNumber = 1;
			int dayOfWeek = 6;
			int dayOfMonth = 1;
			dayOfMonth -= firstDay;

			sb.AppendLine("	<table " + sCssClassTable + "> ");
			sb.AppendLine("		<caption id=\"" + this.ElementId + "-caption\"  " + sCssClassCaption + "> "
					+ "<a href=\"" + thisMonth.DateURL + "\">" + this.ThisMonth.Date.ToString("MMMM yyyy") + "</a> </caption>");

			sb.AppendLine("	<thead id=\"" + this.ElementId + "-head\" " + sCssClassDayHead + ">");
			sb.AppendLine("		<tr>");
			sb.AppendLine("			<th scope=\"col\">SU</th>");
			sb.AppendLine("			<th scope=\"col\">M</th>");
			sb.AppendLine("			<th scope=\"col\">TU</th>");
			sb.AppendLine("			<th scope=\"col\">W</th>");
			sb.AppendLine("			<th scope=\"col\">TR</th>");
			sb.AppendLine("			<th scope=\"col\">F</th>");
			sb.AppendLine("			<th scope=\"col\">SA</th>");
			sb.AppendLine("		</tr>");
			sb.AppendLine("	</thead>");

			sb.AppendLine("		<tbody id=\"" + this.ElementId + "-body\"  " + sCssClassTableBody + ">");
			while ((dayOfMonth <= daysInMonth) && (dayOfMonth <= 31) && (dayOfMonth >= -7)) {
				for (int dayIndex = 0; dayIndex <= dayOfWeek; dayIndex++) {
					if (dayIndex == 0) {
						sb.AppendLine("			<tr id=\"" + this.ElementId + "-week" + weekNumber.ToString() + "\"> ");
						weekNumber++;
					}

					DateTime cellDate = DateTime.MinValue;

					if ((dayOfMonth >= 1) && (dayOfMonth <= daysInMonth)) {
						cellDate = new DateTime(yearNumber, monthNumber, dayOfMonth);

						var cal = (from n in lstCalendar
								   where n.PostDate.Date == cellDate.Date
								   select n).FirstOrDefault();

						string sTD = "<td";
						if (cellDate.Date == SiteData.CurrentSite.Now.Date) {
							sTD = "<td id=\"today\"";
						}

						if (cal != null) {
							sb.Append("			" + sTD + " " + sCssClassDateLink + "> ");
							sb.Append("<a href=\"" + cal.DateURL + "\"> " + cellDate.Day.ToString() + " </a>");
						} else {
							sb.Append("			" + sTD + "> ");
							sb.Append(cellDate.Day.ToString());
						}
						sb.Append(" </td>");
					} else {
						sb.Append("			<td class=\"pad\"> </td>");
					}

					dayOfMonth++;

					if (dayIndex == dayOfWeek) {
						sb.AppendLine("		</tr>");
					}
				}
			}
			sb.AppendLine("		</tbody>");

			// as a bot crawler abuse stopper

			sb.AppendLine("		<tfoot id=\"" + this.ElementId + "-foot\" " + sCssClassTableFoot + ">");
			sb.AppendLine("		<tr>");
			sb.AppendLine("			<td colspan=\"3\" id=\"prev\" class=\"cal-prev\">");
			if (lastMonth.TallyDate >= SiteData.CurrentSite.Now.AddYears(-5)) {
				sb.AppendLine("				<a href=\"" + lastMonth.DateURL + "\">&laquo; " + lastMonth.TallyDate.ToString("MMM") + "</a>");
			}
			sb.AppendLine("			</td>");
			sb.AppendLine("			<td class=\"pad\"> &nbsp; </td>");
			sb.AppendLine("			<td colspan=\"3\" id=\"next\" class=\"cal-prev\">");
			if (nextMonth.TallyDate <= SiteData.CurrentSite.Now.AddYears(5)) {
				sb.AppendLine("				<a href=\"" + nextMonth.DateURL + "\">" + nextMonth.TallyDate.ToString("MMM") + " &raquo;</a>");
			}
			sb.AppendLine("			</td>");
			sb.AppendLine("		</tr>");
			sb.AppendLine("		</tfoot>");

			sb.AppendLine("	</table>");

			sb.AppendLine("</div> ");

			return ControlUtilities.HtmlFormat(sb);
		}

		public string GetHtml() {
			return ToHtmlString();
		}

		public HtmlString RenderHtml() {
			return new HtmlString(ToHtmlString());
		}
	}
}