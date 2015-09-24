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

	internal class SearchParameterObject {

		public SearchParameterObject() {
			this.DateCompare = DateTime.UtcNow;

			this.DateBegin = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
			this.DateEnd = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;

			this.SiteID = Guid.Empty;
			this.ParentContentID = Guid.Empty;
			this.RootContentID = Guid.Empty;
			this.ActiveOnly = true;
		}

		public Guid ItemID { get; set; }
		public Guid SiteID { get; set; }
		public Guid UserId { get; set; }
		public string KeyType { get; set; }

		public Guid ContentTypeID { get; set; }
		public ContentPageType.PageType ContentType { get; set; }

		public Guid? ParentContentID { get; set; }
		public string ParentFileName { get; set; }

		public bool ActiveOnly { get; set; }
		public Guid RootContentID { get; set; }
		public string FileName { get; set; }
		public Guid ContentID { get; set; }

		public DateTime DateBegin { get; set; }
		public DateTime DateEnd { get; set; }
		public DateTime DateCompare { get; set; }

		public Guid ItemSlugID { get; set; }
		public string ItemSlug { get; set; }

		public Guid GuidParm1 { get; set; }
		public Guid GuidParm2 { get; set; }
		public Guid GuidParm3 { get; set; }
		public Guid GuidParm4 { get; set; }
		public Guid GuidParm5 { get; set; }

		public string StringParm1 { get; set; }
		public string StringParm2 { get; set; }
		public string StringParm3 { get; set; }
		public string StringParm4 { get; set; }
		public string StringParm5 { get; set; }
	}
}