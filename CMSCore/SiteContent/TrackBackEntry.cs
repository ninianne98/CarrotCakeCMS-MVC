using Carrotware.CMS.Data;
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

namespace Carrotware.CMS.Core {

	public class TrackBackEntry : IDisposable {
		private CarrotCMSDataContext _db = CarrotCMSDataContext.Create();
		//private CarrotCMSDataContext _db = CompiledQueries.dbConn;

		public Guid TrackbackQueueID { get; set; }
		public Guid Root_ContentID { get; set; }
		public string TrackBackURL { get; set; }
		public string TrackBackResponse { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime ModifiedDate { get; set; }
		public bool TrackedBack { get; set; }

		public TrackBackEntry() { }

		internal TrackBackEntry(vw_carrot_TrackbackQueue c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);
				this.TrackbackQueueID = c.TrackbackQueueID;
				this.Root_ContentID = c.Root_ContentID;
				this.TrackBackURL = c.TrackBackURL;
				this.TrackBackResponse = c.TrackBackResponse;
				this.CreateDate = site.ConvertUTCToSiteTime(c.CreateDate);
				this.ModifiedDate = site.ConvertUTCToSiteTime(c.ModifiedDate);
				this.TrackedBack = c.TrackedBack;
			}
		}

		public static TrackBackEntry Get(Guid TrackbackQueueID) {
			TrackBackEntry _item = null;
			using (var db = CarrotCMSDataContext.Create()) {
				vw_carrot_TrackbackQueue query = CompiledQueries.cqGetTrackbackByID(db, TrackbackQueueID);
				_item = new TrackBackEntry(query);
			}

			return _item;
		}

		public void Save() {
			bool bNew = false;
			carrot_TrackbackQueue s = CompiledQueries.cqGetTrackbackTblByID(_db, this.TrackbackQueueID);

			if (s == null) {
				s = new carrot_TrackbackQueue();
				s.TrackbackQueueID = Guid.NewGuid();
				s.Root_ContentID = this.Root_ContentID;
				s.CreateDate = DateTime.UtcNow;
				s.TrackBackURL = this.TrackBackURL.Trim();
				bNew = true;
			}

			s.ModifiedDate = DateTime.UtcNow;
			s.TrackBackResponse = this.TrackBackResponse;
			s.TrackedBack = this.TrackedBack;

			if (bNew) {
				_db.carrot_TrackbackQueues.InsertOnSubmit(s);
			}

			this.TrackbackQueueID = s.TrackbackQueueID;

			_db.SubmitChanges();
		}

		public static List<TrackBackEntry> GetPageTrackBackList(Guid rootContentID) {
			List<TrackBackEntry> _types = null;

			using (var db = CarrotCMSDataContext.Create()) {
				IQueryable<vw_carrot_TrackbackQueue> query = CompiledQueries.cqGetTrackbackByRootID(db, rootContentID);

				_types = (from d in query.ToList()
						  select new TrackBackEntry(d)).ToList();
			}

			return _types;
		}

		public static List<TrackBackEntry> GetTrackBackQueue(Guid rootContentID) {
			List<TrackBackEntry> _types = null;

			using (var db = CarrotCMSDataContext.Create()) {
				IQueryable<vw_carrot_TrackbackQueue> query = CompiledQueries.cqGetTrackbackByRootIDUnTracked(db, rootContentID);

				_types = (from d in query.ToList()
						  select new TrackBackEntry(d)).ToList();
			}

			return _types;
		}

		public static List<TrackBackEntry> GetTrackBackSiteQueue(Guid siteID) {
			List<TrackBackEntry> _types = null;

			using (var db = CarrotCMSDataContext.Create()) {
				IQueryable<vw_carrot_TrackbackQueue> query = CompiledQueries.cqGetTrackbackBySiteIDUnTracked(db, siteID);

				_types = (from d in query.ToList()
						  select new TrackBackEntry(d)).ToList();
			}

			return _types;
		}

		public static List<TrackBackEntry> FindTrackbackByURL(Guid rootContentID, string sURL) {
			List<TrackBackEntry> _types = null;

			using (var db = CarrotCMSDataContext.Create()) {
				IQueryable<vw_carrot_TrackbackQueue> query = CompiledQueries.cqGetTrackbackByRootIDAndUrl(db, rootContentID, sURL);

				_types = (from d in query.ToList()
						  select new TrackBackEntry(d)).ToList();
			}

			return _types;
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is TrackBackEntry) {
				TrackBackEntry p = (TrackBackEntry)obj;
				return (this.TrackbackQueueID == p.TrackbackQueueID
						&& this.Root_ContentID == p.Root_ContentID
						&& this.TrackBackURL.ToLowerInvariant() == p.TrackBackURL.ToLowerInvariant());
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return TrackbackQueueID.GetHashCode() ^ TrackBackURL.GetHashCode();
		}

		#region IDisposable Members

		public void Dispose() {
			if (_db != null) {
				_db.Dispose();
			}
		}

		#endregion IDisposable Members
	}
}