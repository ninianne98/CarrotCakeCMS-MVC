using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

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

	public class ContentPageType : IDisposable {
		private CarrotCMSDataContext db = CarrotCMSDataContext.Create();

		public enum PageType {
			Unknown,
			BlogEntry,
			ContentEntry
		}

		public Guid ContentTypeID { get; set; }
		public string ContentTypeValue { get; set; }

		private static string keyContentPageType = "cms_ContentPageTypeList";

		public static List<ContentPageType> ContentPageTypeList {
			get {
				List<ContentPageType> _types = new List<ContentPageType>();

				bool bCached = false;

				try {
					_types = (List<ContentPageType>)HttpContext.Current.Cache[keyContentPageType];
					if (_types != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
						IQueryable<carrot_ContentType> query = CompiledQueries.cqGetContentTypes(_db);

						_types = (from d in query.ToList()
								  select new ContentPageType {
									  ContentTypeID = d.ContentTypeID,
									  ContentTypeValue = d.ContentTypeValue
								  }).ToList();
					}

					_types.Add(new ContentPageType {
						ContentTypeID = Guid.Empty,
						ContentTypeValue = "Unknown"
					});

					HttpContext.Current.Cache.Insert(keyContentPageType, _types, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}

				return _types;
			}
		}

		public static PageType GetTypeByID(Guid contentTypeID) {
			ContentPageType _type = ContentPageTypeList.Where(t => t.ContentTypeID == contentTypeID).FirstOrDefault();

			return GetTypeByName(_type.ContentTypeValue);
		}

		public static PageType GetTypeByName(string contentTypeValue) {
			PageType pt = PageType.ContentEntry;

			if (!String.IsNullOrEmpty(contentTypeValue)) {
				try {
					pt = (PageType)Enum.Parse(typeof(PageType), contentTypeValue, true);
				} catch (Exception ex) { }
			}

			return pt;
		}

		public static Guid GetIDByType(PageType contentType) {
			ContentPageType _type = ContentPageTypeList.Where(t => t.ContentTypeValue.ToLower() == contentType.ToString().ToLower()).FirstOrDefault();

			return _type.ContentTypeID;
		}

		#region IDisposable Members

		public void Dispose() {
			if (db != null) {
				db.Dispose();
			}
		}

		#endregion IDisposable Members
	}
}