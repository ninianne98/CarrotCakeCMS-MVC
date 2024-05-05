using System;
using System.ComponentModel.DataAnnotations;
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

namespace CarrotCake.CMS.Plugins.PhotoGallery {

	public class GalleryImageEntry {

		public GalleryImageEntry() { }

		internal GalleryImageEntry(tblGalleryImage gal) {
			if (gal != null) {
				this.GalleryID = gal.GalleryID.Value;
				this.GalleryImageID = gal.GalleryImageID;

				this.GalleryImage = gal.GalleryImage;
				this.ImageOrder = gal.ImageOrder.Value;

				this.ValidateGalleryImage();
			}
		}

		[Key]
		public Guid GalleryID { get; set; }

		public Guid GalleryImageID { get; set; }

		[Required]
		public string GalleryImage { get; set; }

		public int ImageOrder { get; set; }

		public void ValidateGalleryImage() {
			if (!string.IsNullOrEmpty(this.GalleryImage)) {
				if (this.GalleryImage.Contains("../") || this.GalleryImage.Contains(@"..\")) {
					throw new Exception("Cannot use relative paths.");
				}
				if (this.GalleryImage.Contains(":")) {
					throw new Exception("Cannot specify drive letters.");
				}
				if (this.GalleryImage.Contains("//") || this.GalleryImage.Contains(@"\\")) {
					throw new Exception("Cannot use UNC paths.");
				}
				if (this.GalleryImage.Contains("<") || this.GalleryImage.Contains(">")) {
					throw new Exception("Cannot include html tags.");
				}
			}
		}

		public void Save() {
			using (var db = PhotoGalleryDataContext.GetDataContext()) {
				var gal = (from c in db.tblGalleryImages
						   where c.GalleryImageID == this.GalleryImageID
						   select c).FirstOrDefault();

				if (gal == null || this.GalleryID == Guid.Empty) {
					gal = new tblGalleryImage();
					gal.GalleryID = this.GalleryID;
					gal.GalleryImageID = Guid.NewGuid();
				}

				gal.GalleryImage = this.GalleryImage;
				gal.ImageOrder = this.ImageOrder;

				if (gal.GalleryImageID != this.GalleryImageID) {
					db.tblGalleryImages.InsertOnSubmit(gal);
				}

				this.ValidateGalleryImage();

				db.SubmitChanges();

				this.GalleryImageID = gal.GalleryImageID;
			}
		}

		public override string ToString() {
			return this.GalleryImage;
		}

		public override bool Equals(object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is GalleryImageEntry) {
				GalleryImageEntry p = (GalleryImageEntry)obj;
				return (this.GalleryImageID == p.GalleryImageID)
						&& (this.GalleryID == p.GalleryID)
						&& (this.GalleryImage == p.GalleryImage);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.GalleryImageID.GetHashCode() ^ this.GalleryID.GetHashCode() ^ this.GalleryImage.GetHashCode();
		}
	}
}