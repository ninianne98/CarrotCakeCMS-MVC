using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public class Calendar : BaseTwoPartWebComponent {

		public Calendar() {
			this.CellColor = ColorTranslator.FromHtml("#ffffff");
			this.CellBackground = ColorTranslator.FromHtml("#00509F");
			this.WeekdayColor = ColorTranslator.FromHtml("#000000");
			this.WeekdayBackground = ColorTranslator.FromHtml("#00C87F");
			this.TodayColor = ColorTranslator.FromHtml("#FFFF80");
			this.TodayBackground = ColorTranslator.FromHtml("#800080");
			this.TodaySelectBorder = ColorTranslator.FromHtml("#FFFF80");
			this.NormalColor = ColorTranslator.FromHtml("#004040");
			this.NormalBackground = ColorTranslator.FromHtml("#D8D8EB");
			this.NormalSelectBorder = ColorTranslator.FromHtml("#FF0080");
			this.TodayLink = ColorTranslator.FromHtml("#FFFF80");
			this.NormalLink = ColorTranslator.FromHtml("#004040");
			this.CellColor = ColorTranslator.FromHtml("#ffffff");
			this.CellColor = ColorTranslator.FromHtml("#ffffff");
			this.CellColor = ColorTranslator.FromHtml("#ffffff");
			this.CellColor = ColorTranslator.FromHtml("#ffffff");
			this.JavascriptForDate = String.Empty;
			this.OverrideCssFile = String.Empty;
		}

		public Color CellColor { get; set; }
		public Color CellBackground { get; set; }
		public Color WeekdayColor { get; set; }
		public Color WeekdayBackground { get; set; }
		public Color TodayColor { get; set; }
		public Color TodayBackground { get; set; }
		public Color TodaySelectBorder { get; set; }
		public Color NormalColor { get; set; }
		public Color NormalBackground { get; set; }
		public Color NormalSelectBorder { get; set; }
		public Color TodayLink { get; set; }
		public Color NormalLink { get; set; }
		public string JavascriptForDate { get; set; }
		public string OverrideCssFile { get; set; }
		public int MonthNumber { get; private set; }
		public int YearNumber { get; private set; }

		private DateTime _date = DateTime.Today;

		public DateTime CalendarDate {
			get {
				this.YearNumber = _date.Year;
				this.MonthNumber = _date.Month;
				return _date;
			}
			set {
				_date = value;
				this.YearNumber = _date.Year;
				this.MonthNumber = _date.Month;
			}
		}

		public string HilightDates { get; set; }

		public string ElementId { get; set; }

		public List<DateTime> HilightDateList { get; set; }

		public override string GetBody() {
			StringBuilder sb = new StringBuilder();
			string CtrlID = this.ElementId;

			sb.AppendLine();

			DateTime today = DateTime.Today.Date;
			DateTime thisMonth = DateTime.Today.Date;

			try {
				thisMonth = new DateTime(YearNumber, MonthNumber, 15);
			} catch { }

			DateTime firstOfMonth = thisMonth.AddDays(1 - thisMonth.Day);

			DateTime nextMonth = thisMonth.AddMonths(1);
			DateTime prevMonth = thisMonth.AddMonths(-1);

			int iFirstDay = (int)firstOfMonth.DayOfWeek;
			TimeSpan ts = firstOfMonth.AddMonths(1) - firstOfMonth;
			int iDaysInMonth = ts.Days;

			string MonthName = thisMonth.ToString("MMMM") + "  " + thisMonth.Year.ToString();

			int dayOfWeek = 6;
			int dayOfMonth = 1;
			dayOfMonth -= iFirstDay;

			List<DateTime> dates = new List<DateTime>();

			if (this.HilightDateList != null && this.HilightDateList.Any()) {
				dates = this.HilightDateList;
			} else {
				List<string> lstDates = new List<string>();

				if (!String.IsNullOrEmpty(this.HilightDates)) {
					lstDates = this.HilightDates.Split(';').AsEnumerable().ToList();
				}
				dates = (from dd in lstDates select Convert.ToDateTime(dd)).ToList();
			}

			int WeekNumber = 1;

			while ((dayOfMonth <= iDaysInMonth) && (dayOfMonth <= 31) && (dayOfMonth >= -7)) {
				for (int DayIndex = 0; DayIndex <= dayOfWeek; DayIndex++) {
					if (DayIndex == 0) {
						sb.Append("<tr class=\"weekdayrow\" id=\"" + CtrlID + "-weekRow" + WeekNumber.ToString() + "\">\n");
						WeekNumber++;
					}

					string strCaption = "&nbsp;";
					string sClass = "normal";
					DateTime cellDate = DateTime.MinValue;

					if ((dayOfMonth >= 1) && (dayOfMonth <= iDaysInMonth)) {
						cellDate = new DateTime(YearNumber, MonthNumber, dayOfMonth);
						if (!String.IsNullOrEmpty(JavascriptForDate)) {
							strCaption = "&nbsp;<a href=\"javascript:" + JavascriptForDate + "('" + cellDate.ToString("yyyy-MM-dd") + "')\">" + dayOfMonth.ToString() + "&nbsp;";
						} else {
							strCaption = "&nbsp;" + dayOfMonth.ToString() + "&nbsp;";
						}
					}

					if (strCaption != "&nbsp;") {
						cellDate = new DateTime(YearNumber, MonthNumber, dayOfMonth);
						if (cellDate == today) {
							sClass = "today";
						}

						IEnumerable<DateTime> copyRows = (from c in dates
														  where c == cellDate.Date
														  select c);
						if (copyRows.Count() > 0) {
							sClass = sClass + "sel";
						}
					}

					dayOfMonth++;

					string cell = "\t<td id=\"" + CtrlID + "-cellDay" + dayOfMonth.ToString() + "\" class=\"" + sClass + "\">" + strCaption + "</td>\n";
					sb.Append(cell);

					if (DayIndex == dayOfWeek) {
						sb.Append("</tr>\n");
					}
				}
			}

			sb.AppendLine("<table  id=\"" + CtrlID + "-CalTable\" class=\"calendarGrid\" cellspacing=\"0\" cellpadding=\"3\" align=\"center\" border=\"1\">");
			sb.AppendLine("	<tr class=\"calendarheadrow\">");
			sb.AppendLine("		<td class=\"head\" colspan=\"7\">");
			sb.AppendLine("			<table class=\"innerhead\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" border=\"0\">");
			sb.AppendLine("				<tr> <td class=\"head normaltext\"> &nbsp; </td> </tr>");
			sb.AppendLine("				<tr> <td class=\"head headtext\"> " + MonthName + " </td> </tr>");
			sb.AppendLine("				<tr> <td class=\"head normaltext\"> &nbsp; </td> </tr>");
			sb.AppendLine("			</table>");
			sb.AppendLine("		</td>");
			sb.AppendLine("	</tr>");
			sb.AppendLine();
			sb.AppendLine("	<tr class=\"weekday\">");
			sb.AppendLine("		<td class=\"weekday\" width=\"38\"> SU </td>");
			sb.AppendLine("		<td class=\"weekday\" width=\"38\"> M </td>");
			sb.AppendLine("		<td class=\"weekday\" width=\"38\"> TU </td>");
			sb.AppendLine("		<td class=\"weekday\" width=\"38\"> W </td>");
			sb.AppendLine("		<td class=\"weekday\" width=\"38\"> TR </td>");
			sb.AppendLine("		<td class=\"weekday\" width=\"38\"> F </td>");
			sb.AppendLine("		<td class=\"weekday\" width=\"38\"> SA </td>");
			sb.AppendLine("	</tr>");
			sb.AppendLine();

			sb.AppendLine("</table>");

			return sb.ToString();
		}

		public override string GetHead() {
			if (String.IsNullOrEmpty(this.OverrideCssFile)) {
				string sCSS = String.Empty;

				Assembly _assembly = Assembly.GetExecutingAssembly();

				using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream("Carrotware.Web.UI.Components.calendar.txt"))) {
					sCSS = oTextStream.ReadToEnd();
				}

				sCSS = sCSS.Replace("{WEEKDAY_CHEX}", ColorTranslator.ToHtml(this.WeekdayColor));
				sCSS = sCSS.Replace("{WEEKDAY_BGHEX}", ColorTranslator.ToHtml(this.WeekdayBackground));
				sCSS = sCSS.Replace("{CELL_CHEX}", ColorTranslator.ToHtml(this.CellColor));
				sCSS = sCSS.Replace("{CELL_BGHEX}", ColorTranslator.ToHtml(this.CellBackground));

				sCSS = sCSS.Replace("{TODAY_CHEX}", ColorTranslator.ToHtml(this.TodayColor));
				sCSS = sCSS.Replace("{TODAY_BGHEX}", ColorTranslator.ToHtml(this.TodayBackground));
				sCSS = sCSS.Replace("{TODAYSEL_BDR}", ColorTranslator.ToHtml(this.TodaySelectBorder));
				sCSS = sCSS.Replace("{TODAY_LNK}", ColorTranslator.ToHtml(this.TodayLink));

				sCSS = sCSS.Replace("{NORMAL_CHEX}", ColorTranslator.ToHtml(this.NormalColor));
				sCSS = sCSS.Replace("{NORMAL_BGHEX}", ColorTranslator.ToHtml(this.NormalBackground));
				sCSS = sCSS.Replace("{NORMALSEL_BDR}", ColorTranslator.ToHtml(this.NormalSelectBorder));
				sCSS = sCSS.Replace("{NORMAL_LNK}", ColorTranslator.ToHtml(this.NormalLink));

				sCSS = sCSS.Replace("{CALENDAR_ID}", "#" + this.ElementId);
				sCSS = "\r\n<style type=\"text/css\">\r\n" + sCSS + "\r\n</style>\r\n";

				return sCSS;
			} else {
				return "<link rel=\"stylesheet\" href=\"" + this.OverrideCssFile + "\" type=\"text/css\" />";
			}
		}
	}
}