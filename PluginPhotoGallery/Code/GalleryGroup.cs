using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CarrotCake.CMS.Plugins.PhotoGallery {

	public class GalleryGroup {

		public GalleryGroup() { }

		internal GalleryGroup(tblGallery gal) {
			if (gal != null) {
				this.GalleryID = gal.GalleryID;
				this.SiteID = gal.SiteID.Value;

				this.GalleryTitle = gal.GalleryTitle;

				using (var gh = new GalleryHelper(this.SiteID)) {
					this.GalleryImages = gh.GalleryImageEntryListGetByGalleryID(this.GalleryID);
				}
			}
		}

		[Key]
		[Display(Name = "ID")]
		public Guid GalleryID { get; set; }

		[Display(Name = "Site ID")]
		public Guid SiteID { get; set; }

		[Required]
		[Display(Name = "Gallery Title")]
		public string GalleryTitle { get; set; }

		[Display(Name = "Images")]
		public List<GalleryImageEntry> GalleryImages { get; set; }

		public void Save() {
			using (var db = PhotoGalleryDataContext.GetDataContext()) {
				var gal = (from c in db.tblGalleries
						   where c.GalleryID == this.GalleryID
						   select c).FirstOrDefault();

				if (gal == null || this.GalleryID == Guid.Empty) {
					gal = new tblGallery();
					gal.SiteID = this.SiteID;
					gal.GalleryID = Guid.NewGuid();
				}

				gal.GalleryTitle = this.GalleryTitle;

				if (gal.GalleryID != this.GalleryID) {
					db.tblGalleries.InsertOnSubmit(gal);
				}

				db.SubmitChanges();

				this.GalleryID = gal.GalleryID;
			}
		}

		public override string ToString() {
			return GalleryTitle;
		}

		public override bool Equals(object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is GalleryGroup) {
				var p = (GalleryGroup)obj;
				return (this.GalleryID == p.GalleryID)
						&& (this.SiteID == p.SiteID)
						&& (this.GalleryTitle == p.GalleryTitle);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.GalleryID.GetHashCode() ^ this.SiteID.GetHashCode() ^ this.GalleryTitle.GetHashCode();
		}
	}
}