using CarrotCake.CMS.Plugins.EventCalendarModule.Code;
using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
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

namespace CarrotCake.CMS.Plugins.EventCalendarModule {

	public class CalendarFrequencyHelper {

		public static void SaveFrequencies(CalendarDataContext ctx, CalendarEvent newItem, CalendarEvent oldItem) {
			SiteData site = SiteData.CurrentSite;
			DateTime todayDate = DateTime.UtcNow.Date;

			DateTime newStartDateTime = GetStartDateTimeFromItem(newItem);
			DateTime newEndDateTime = GetEndDateTimeFromItem(newItem);

			List<DateTime> eventDates = GetSequenceDates(todayDate, false, newItem);

			if (newItem.EventStartDate.Date <= todayDate.AddYears(-1)
				|| newItem.EventStartDate.Date >= todayDate.AddMonths(6)) {
				eventDates = eventDates.Union(GetSequenceDates(todayDate, true, newItem)).Distinct().ToList();
			}

			List<DateTime> eventUTCDates = (from d in eventDates select site.ConvertSiteTimeToUTC(d)).ToList();

			if (oldItem.SiteID == Guid.Empty && oldItem.CalendarFrequencyID == Guid.Empty) {
				InsertEventsFromList(ctx, newItem, eventDates);
			} else {
				DateTime oldStartDateTime = GetStartDateTimeFromItem(oldItem);

				int iDays = GetDateDiffDays(newStartDateTime, oldStartDateTime);

				TimeSpan newTS = new TimeSpan();
				if (newItem.EventStartTime.HasValue) {
					newTS = newItem.EventStartTime.Value;
				}
				TimeSpan oldTS = new TimeSpan();
				if (oldItem.EventStartTime.HasValue) {
					oldTS = oldItem.EventStartTime.Value;
				}
				int iMin = GetDateDiffMinutes(newTS, oldTS);

				DateTime dateMax = (from d in eventUTCDates
									orderby d.Date descending
									where d.Date >= DateTime.UtcNow.Date
									select d).FirstOrDefault();

				if (dateMax == DateTime.MinValue) {
					dateMax = DateTime.UtcNow.Date;
				}
				dateMax = site.ConvertSiteTimeToUTC(dateMax).Date;

				DateTime dStart = site.ConvertSiteTimeToUTC(newStartDateTime).Date;
				DateTime dEnd = site.ConvertSiteTimeToUTC(newEndDateTime).Date;

				int iFuture = (from d in eventUTCDates
							   where d.Date >= DateTime.UtcNow.Date
							   select d).Count();

				int iMaxRange = (from c in ctx.carrot_CalendarEvents
								 where c.CalendarEventProfileID == newItem.CalendarEventProfileID
										&& c.EventDate.Date >= dateMax.Date
								 select c.CalendarEventID).Count();

				if (oldItem.Frequency != newItem.Frequency || oldItem.RecursEvery != newItem.RecursEvery
					|| iMin != 0 || iDays != 0 || (iMaxRange != iFuture && iFuture > 0)
					|| oldItem.EventEndDate.Date != newItem.EventEndDate.Date
					|| oldItem.EventStartDate.Date != newItem.EventStartDate.Date) {
					var lstEvents = (from c in ctx.carrot_CalendarEvents
									 orderby c.EventDate
									 where c.CalendarEventProfileID == newItem.CalendarEventProfileID
									 select c).ToList();

					if (iDays != 0) {
						lstEvents.ForEach(x => x.EventDate = site.ConvertUTCToSiteTime(x.EventDate).Date);

						if (newItem.Frequency == FrequencyType.Daily || newItem.Frequency == FrequencyType.Once) {
							lstEvents.ForEach(x => x.EventDate = x.EventDate.Date.AddDays(iDays));
						}

						if (newItem.Frequency == FrequencyType.Weekly) {
							int iDayShift = iDays;
							if (newItem.EventRepeatPattern != null) {
								iDayShift = 7 * GetDateDiffWeeks(newItem.EventStartDate, oldItem.EventStartDate);
							}
							lstEvents.ForEach(x => x.EventDate = x.EventDate.Date.AddDays(iDayShift));
						}

						if (newItem.Frequency == FrequencyType.Monthly || newItem.Frequency == FrequencyType.Yearly) {
							int iMonths = GetDateDiffMonths(newItem.EventStartDate, oldItem.EventStartDate);

							lstEvents.ForEach(x => x.EventDate = x.EventDate.AddMonths(iMonths).Date);
						}

						if (newItem.Frequency == oldItem.Frequency && (newItem.Frequency == FrequencyType.Monthly || newItem.Frequency == FrequencyType.Yearly)) {
							lstEvents.ForEach(x => x.EventDate = CorrectMonthlyYearlyDate(x.EventDate, newItem.EventStartDate.Day).Date);
						}

						lstEvents.ForEach(x => x.EventDate = site.ConvertSiteTimeToUTC(x.EventDate));
					}

					var lstDel = (from l in lstEvents
								  orderby l.EventDate
								  where l.EventDate.Date < dStart || l.EventDate.Date > dEnd
									|| !eventUTCDates.Contains(l.EventDate)
								  select l).ToList();

					lstDel.RemoveAll(l => eventUTCDates.Contains(l.EventDate));

					ctx.carrot_CalendarEvents.DeleteAllOnSubmit(lstDel);

					var lstExist = (from l in lstEvents select site.ConvertUTCToSiteTime(l.EventDate).Date).Distinct().ToList();

					var lstAdd = (from d in eventDates
								  where !lstExist.Contains(d.Date)
								  select d).ToList();

					InsertEventsFromList(ctx, newItem, lstAdd);
				}
			}
		}

		protected static DateTime GetStartDateTimeFromItem(CalendarEvent item) {
			DateTime newStartDateTime = item.EventStartDate;
			if (item.EventStartTime != null) {
				newStartDateTime = CalendarHelper.GetFullDateTime(item.EventStartDate, item.EventStartTime);
			}

			return newStartDateTime;
		}

		protected static DateTime GetEndDateTimeFromItem(CalendarEvent item) {
			DateTime newStartDateTime = item.EventStartDate;
			if (item.EventStartTime != null) {
				newStartDateTime = CalendarHelper.GetFullDateTime(item.EventEndDate, item.EventStartTime);
			}

			return newStartDateTime;
		}

		protected static void InsertEventsFromList(CalendarDataContext ctx, CalendarEvent item, List<DateTime> lstDates) {
			SiteData site = SiteData.CurrentSite;

			foreach (DateTime date in lstDates) {
				carrot_CalendarEvent evt = new carrot_CalendarEvent {
					CalendarEventID = Guid.NewGuid(),
					CalendarEventProfileID = item.CalendarEventProfileID,
					EventDate = site.ConvertSiteTimeToUTC(date),
					IsCancelled = false
				};

				ctx.carrot_CalendarEvents.InsertOnSubmit(evt);
			}
		}

		protected static List<DateTime> GetSequenceDates(DateTime testDate, bool createRecentOnly, CalendarEvent eventProfile) {
			List<DateTime> eventDates = new List<DateTime>();
			DateTime backportDate = testDate.AddMonths(-6).Date;
			DateTime endDateRange = testDate.AddYears(5).Date;

			DateTime dateToAdd = GetStartDateTimeFromItem(eventProfile).Date;

			DateTime oldDate = eventProfile.EventStartDate;
			int iDayOfMonth = oldDate.Day;

			switch (eventProfile.Frequency) {
				case FrequencyType.Once:
					eventDates.Add(dateToAdd.Date);
					break;

				case FrequencyType.Weekly:
					if (eventProfile.EventRepeatPattern == null) {
						while (eventDates.Count < 2000 && dateToAdd.Date <= endDateRange && dateToAdd.Date <= eventProfile.EventEndDate) {
							if (!createRecentOnly || (createRecentOnly && dateToAdd.Date >= backportDate)) {
								eventDates.Add(dateToAdd.Date);
							}
							dateToAdd = dateToAdd.AddDays(7 * eventProfile.RecursEvery);
						}
					} else {
						while (eventDates.Count < 5000 && dateToAdd.Date <= endDateRange && dateToAdd.Date <= eventProfile.EventEndDate) {
							if (eventProfile.DaysValid.Contains(dateToAdd.DayOfWeek)) {
								if (!createRecentOnly || (createRecentOnly && dateToAdd.Date >= backportDate)) {
									eventDates.Add(dateToAdd.Date);
								}
							}

							if (eventProfile.RecursEvery > 1) {
								if (oldDate.DayOfWeek == dateToAdd.AddDays(1).DayOfWeek) {
									dateToAdd = dateToAdd.AddDays((7 * (eventProfile.RecursEvery - 1)) + 1);
								} else {
									dateToAdd = dateToAdd.AddDays(1);
								}
							} else {
								dateToAdd = dateToAdd.AddDays(1);
							}
						}
					}

					break;

				case FrequencyType.Daily:

					while (eventDates.Count < 5000 && dateToAdd.Date <= endDateRange && dateToAdd.Date <= eventProfile.EventEndDate) {
						if (!createRecentOnly || (createRecentOnly && dateToAdd.Date >= backportDate)) {
							eventDates.Add(dateToAdd.Date);
						}
						dateToAdd = dateToAdd.AddDays(1);
					}
					break;

				case FrequencyType.Yearly:

					while (eventDates.Count < 25 && dateToAdd.Date <= endDateRange && dateToAdd.Date <= eventProfile.EventEndDate) {
						if (!createRecentOnly || (createRecentOnly && dateToAdd.Date >= backportDate)) {
							eventDates.Add(dateToAdd.Date);
						}

						dateToAdd = dateToAdd.AddYears(1 * eventProfile.RecursEvery);
						dateToAdd = CorrectMonthlyYearlyDate(dateToAdd, iDayOfMonth);
					}
					break;

				case FrequencyType.Monthly:

					while (eventDates.Count < 360 && dateToAdd.Date <= endDateRange && dateToAdd.Date <= eventProfile.EventEndDate) {
						if (!createRecentOnly || (createRecentOnly && dateToAdd.Date >= backportDate)) {
							eventDates.Add(dateToAdd.Date);
						}

						dateToAdd = dateToAdd.AddMonths(1 * eventProfile.RecursEvery);
						dateToAdd = CorrectMonthlyYearlyDate(dateToAdd, iDayOfMonth);
					}
					break;
			}

			return eventDates;
		}

		protected static DateTime CorrectMonthlyYearlyDate(DateTime dateNew, int dayOfMonth) {
			int iNewDaysInMonth = DateTime.DaysInMonth(dateNew.Year, dateNew.Month);

			if (dateNew.Day != dayOfMonth) {
				if (iNewDaysInMonth >= dayOfMonth) {
					dateNew = new DateTime(dateNew.Year, dateNew.Month, dayOfMonth);
				} else {
					dateNew = new DateTime(dateNew.Year, dateNew.Month, iNewDaysInMonth);
				}
			}

			return dateNew;
		}

		public static int GetDateDiffMonths(DateTime date1, DateTime date2) {
			return ((date1.Year - date2.Year) * 12) + date1.Month - date2.Month;
		}

		public static int GetDateDiffWeeks(DateTime date1, DateTime date2) {
			return GetDateDiffDays(date1, date2) / 7;
		}

		public static int GetDateDiffDays(DateTime date1, DateTime date2) {
			TimeSpan dateOffset = date1.Subtract(date2);
			int iDays = (int)dateOffset.TotalDays;
			return iDays;
		}

		public static int GetDateDiffMinutes(TimeSpan time1, TimeSpan time2) {
			TimeSpan dateOffset = time1.Subtract(time2);
			int iMin = (int)dateOffset.TotalMinutes;
			return iMin;
		}

		//===================================================

		public enum FrequencyType {
			Yearly,
			Weekly,
			Daily,
			Once,
			Monthly,
		}

		private static Dictionary<Guid, string> _types = null;

		public static Dictionary<Guid, string> FrequencyTypeDictionary {
			get {
				if (_types == null) {
					_types = (from c in GetCalendarFrequencies()
							  orderby c.FrequencySortOrder
							  select c).ToList().ToDictionary(k => k.CalendarFrequencyID, v => v.FrequencyValue);
				}

				return _types;
			}
		}

		public static FrequencyType GetFrequencyTypeByID(Guid contentTypeID) {
			KeyValuePair<Guid, string> _type = FrequencyTypeDictionary.Where(t => t.Key == contentTypeID).FirstOrDefault();

			return GetTypeByName(_type.Value);
		}

		public static FrequencyType GetTypeByName(string typeValue) {
			FrequencyType pt = FrequencyType.Daily;

			if (!string.IsNullOrEmpty(typeValue)) {
				try {
					pt = (FrequencyType)Enum.Parse(typeof(FrequencyType), typeValue, true);
				} catch (Exception ex) { }
			}

			return pt;
		}

		public static Guid GetIDByFrequencyType(FrequencyType contentType) {
			KeyValuePair<Guid, string> _type = FrequencyTypeDictionary.Where(t => t.Value.ToLower() == contentType.ToString().ToLower()).FirstOrDefault();

			return _type.Key;
		}

		public static List<carrot_CalendarFrequency> GetCalendarFrequencies() {
			using (var db = CalendarDataContext.GetDataContext()) {
				return (from c in db.carrot_CalendarFrequencies
						orderby c.FrequencySortOrder
						select c).ToList();
			}
		}
	}
}