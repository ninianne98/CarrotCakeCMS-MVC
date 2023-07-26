using CarrotCake.CMS.Plugins.EventCalendarModule.Code;
using System;
using System.Collections.Generic;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace CarrotCake.CMS.Plugins.EventCalendarModule {

	public class CalendarEvent {

		public CalendarEvent() { }

		internal CalendarEvent(carrot_CalendarEventProfile p) {
			if (p != null) {
				this.CalendarEventProfileID = p.CalendarEventProfileID;
				this.CalendarFrequencyID = p.CalendarFrequencyID;
				this.CalendarEventCategoryID = p.CalendarEventCategoryID;
				this.EventStartDate = p.EventStartDate;
				this.EventStartTime = p.EventStartTime;
				this.EventEndDate = p.EventEndDate;
				this.EventEndTime = p.EventEndTime;
				this.EventTitle = p.EventTitle;
				this.EventDetail = p.EventDetail;
				this.EventRepeatPattern = p.EventRepeatPattern;
				this.RecursEvery = p.RecursEvery;
				this.IsCancelled = p.IsCancelled;
				this.IsCancelledPublic = p.IsCancelledPublic;
				this.IsAllDayEvent = p.IsAllDayEvent;
				this.IsPublic = p.IsPublic;
				this.IsAnnualHoliday = p.IsAnnualHoliday;
				this.IsHoliday = p.IsHoliday;

				this.SiteID = p.SiteID;

				this.Frequency = CalendarFrequencyHelper.GetFrequencyTypeByID(p.CalendarFrequencyID);
			}
		}

		public Guid CalendarEventProfileID { get; set; }
		public Guid CalendarFrequencyID { get; set; }
		public Guid CalendarEventCategoryID { get; set; }
		public DateTime EventStartDate { get; set; }
		public TimeSpan? EventStartTime { get; set; }
		public DateTime EventEndDate { get; set; }
		public TimeSpan? EventEndTime { get; set; }
		public string EventTitle { get; set; }
		public string EventDetail { get; set; }
		public int? EventRepeatPattern { get; set; }
		public int RecursEvery { get; set; }
		public bool IsCancelled { get; set; }
		public bool IsCancelledPublic { get; set; }
		public bool IsAllDayEvent { get; set; }
		public bool IsPublic { get; set; }
		public bool IsAnnualHoliday { get; set; }
		public bool IsHoliday { get; set; }
		public Guid SiteID { get; set; }

		public CalendarFrequencyHelper.FrequencyType Frequency { get; set; }

		public List<DayOfWeek> DaysValid {
			get {
				List<DayOfWeek> lst = new List<DayOfWeek>();
				if (this.EventRepeatPattern != null) {
					int daysValid = EventRepeatPattern.Value;

					if ((daysValid & (int)DaysInWeek.Sunday) != 0) {
						lst.Add(DayOfWeek.Sunday);
					}
					if ((daysValid & (int)DaysInWeek.Monday) != 0) {
						lst.Add(DayOfWeek.Monday);
					}
					if ((daysValid & (int)DaysInWeek.Tuesday) != 0) {
						lst.Add(DayOfWeek.Tuesday);
					}
					if ((daysValid & (int)DaysInWeek.Wednesday) != 0) {
						lst.Add(DayOfWeek.Wednesday);
					}
					if ((daysValid & (int)DaysInWeek.Thursday) != 0) {
						lst.Add(DayOfWeek.Thursday);
					}
					if ((daysValid & (int)DaysInWeek.Friday) != 0) {
						lst.Add(DayOfWeek.Friday);
					}
					if ((daysValid & (int)DaysInWeek.Saturday) != 0) {
						lst.Add(DayOfWeek.Saturday);
					}
				}
				return lst;
			}
		}

		public enum DaysInWeek : short {
			Monday = 1,
			Tuesday = 2,
			Wednesday = 4,
			Thursday = 8,
			Friday = 16,
			Saturday = 32,
			Sunday = 64
		}
	}
}