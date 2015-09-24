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
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	public class PostCalendar : IWebComponent, IHtmlString {

		public PostCalendar() {
			this.ElementId = "cal";
		}

		protected SiteNavHelper navHelper = new SiteNavHelper();

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
			StringBuilder sb = new StringBuilder();

			List<ContentDateLinks> lstCalendar = GetDates();

			sb.AppendLine();

			string sCSS = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClass)) {
				sCSS = " class=\"" + this.CssClass + "\" ";
			}

			string sCssClassTable = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClassTable)) {
				sCssClassTable = " class=\"" + this.CssClassTable + "\" ";
			}
			string sCssClassCaption = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClassCaption)) {
				sCssClassCaption = " class=\"" + this.CssClassCaption + "\" ";
			}
			string sCssClassDayHead = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClassDayHead)) {
				sCssClassDayHead = " class=\"" + this.CssClassDayHead + "\" ";
			}
			string sCssClassTableBody = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClassTableBody)) {
				sCssClassTableBody = " class=\"" + this.CssClassTableBody + "\" ";
			}
			string sCssClassDateLink = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClassDateLink)) {
				sCssClassDateLink = " class=\"" + this.CssClassDateLink + "\" ";
			}
			string sCssClassTableFoot = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClassTableFoot)) {
				sCssClassTableFoot = " class=\"" + this.CssClassTableFoot + "\" ";
			}

			ContentDateTally lastMonth = new ContentDateTally { TallyDate = this.ThisMonth.AddMonths(-1), TheSite = SiteData.CurrentSite };
			ContentDateTally nextMonth = new ContentDateTally { TallyDate = this.ThisMonth.AddMonths(1), TheSite = SiteData.CurrentSite };

			sb.AppendLine("<div" + sCSS + " id=\"" + this.ElementId + "\"> ");

			if (!String.IsNullOrEmpty(this.CalendarHead)) {
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
			sb.AppendLine("		<caption id=\"" + this.ElementId + "-caption\"  " + sCssClassCaption + "> " + this.ThisMonth.Date.ToString("MMMM yyyy") + " </caption>");

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
				for (int DayIndex = 0; DayIndex <= dayOfWeek; DayIndex++) {
					if (DayIndex == 0) {
						sb.AppendLine("			<tr id=\"" + this.ElementId + "-week" + weekNumber.ToString() + "\"> ");
						weekNumber++;
					}

					DateTime cellDate = DateTime.MinValue;

					if ((dayOfMonth >= 1) && (dayOfMonth <= daysInMonth)) {
						cellDate = new DateTime(yearNumber, monthNumber, dayOfMonth);

						string sTD = "<td";
						if (cellDate.Date == SiteData.CurrentSite.Now.Date) {
							sTD = "<td id=\"today\"";
						}

						ContentDateLinks cal = (from n in lstCalendar
												where n.PostDate.Date == cellDate.Date
												select n).FirstOrDefault();
						if (cal != null) {
							sb.AppendLine("			" + sTD + " " + sCssClassDateLink + ">");
							sb.AppendLine("				<a href=\"" + cal.DateURL + "\"> " + cellDate.Day.ToString() + " </a>");
						} else {
							sb.AppendLine("			" + sTD + ">");
							sb.AppendLine("				" + cellDate.Day.ToString() + " ");
						}
						sb.AppendLine("			</td>");
					} else {
						sb.AppendLine("			<td class=\"pad\"> </td>");
					}

					dayOfMonth++;

					if (DayIndex == dayOfWeek) {
						sb.AppendLine("		</tr>");
					}
				}
			}
			sb.AppendLine("		</tbody>");

			// as a bot crawler abuse stopper

			sb.AppendLine("		<tfoot id=\"" + this.ElementId + "-foot\" " + sCssClassTableFoot + ">");
			sb.AppendLine("		<tr>");
			sb.AppendLine("			<td colspan=\"3\" id=\"prev\" class=\"cal-prev\">");
			if (lastMonth.TallyDate >= SiteData.CurrentSite.Now.AddYears(-3)) {
				sb.AppendLine("				<a href=\"" + lastMonth.DateURL + "\">&laquo; " + lastMonth.TallyDate.ToString("MMM") + "</a>");
			}
			sb.AppendLine("			</td>");
			sb.AppendLine("			<td class=\"pad\"> &nbsp; </td>");
			sb.AppendLine("			<td colspan=\"3\" id=\"next\" class=\"cal-prev\">");
			if (nextMonth.TallyDate <= SiteData.CurrentSite.Now.AddYears(3)) {
				sb.AppendLine("				<a href=\"" + nextMonth.DateURL + "\">" + nextMonth.TallyDate.ToString("MMM") + " &raquo;</a>");
			}
			sb.AppendLine("			</td>");
			sb.AppendLine("		</tr>");
			sb.AppendLine("		</tfoot>");

			sb.AppendLine("	</table>");

			sb.AppendLine("</div> ");

			return sb.ToString();
		}

		public string GetHtml() {
			return ToHtmlString();
		}

		public HtmlString RenderHtml() {
			return new HtmlString(ToHtmlString());
		}
	}
}