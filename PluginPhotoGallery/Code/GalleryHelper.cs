using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CarrotCake.CMS.Plugins.PhotoGallery {

	public class GalleryHelper : IDisposable {
		private PhotoGalleryDataContext _db;
		public Guid SiteID { get; set; }

		public GalleryHelper(Guid siteID) {
			this.SiteID = siteID;

			_db = PhotoGalleryDataContext.GetDataContext();
		}

		public GalleryImageEntry GalleryImageEntryGetByID(Guid galleryImageID) {
			GalleryImageEntry ge = (from c in _db.tblGalleryImages
									where c.GalleryImageID == galleryImageID
									select new GalleryImageEntry(c)).FirstOrDefault();

			return ge;
		}

		public List<GalleryImageEntry> GalleryImageEntryListGetByGalleryID(Guid galleryID) {
			return (from c in _db.tblGalleryImages
					where c.GalleryID == galleryID
					select new GalleryImageEntry(c)).ToList();
		}

		public GalleryImageEntry GalleryImageEntryGetByFilename(Guid galleryID, string galleryImage) {
			return (from c in _db.tblGalleryImages
					where c.GalleryID == galleryID
					&& c.GalleryImage.ToLower() == galleryImage.ToLower()
					orderby c.ImageOrder ascending
					select new GalleryImageEntry(c)).FirstOrDefault();
		}

		public void GalleryImageCleanup(Guid galleryID, List<string> lst) {
			var lstDel = (from g in _db.tblGalleryImages
						  where g.GalleryID == galleryID
						  && !lst.Contains(g.GalleryImage.ToLower())
						  select g).ToList();

			_db.tblGalleryImages.DeleteAllOnSubmit(lstDel);

			_db.SubmitChanges();
		}

		public List<GalleryMetaData> GetGalleryMetaDataListByGalleryID(Guid galleryID) {
			return (from g in _db.tblGalleryImageMetas
					join gg in _db.tblGalleryImages on g.GalleryImage.ToLower() equals gg.GalleryImage.ToLower()
					where g.SiteID == this.SiteID
						&& gg.GalleryID == galleryID
					select new GalleryMetaData(g)).ToList();
		}

		public GalleryGroup GalleryGroupGetByID(Guid galleryID) {
			return (from c in _db.tblGalleries
					where c.SiteID == this.SiteID
					&& c.GalleryID == galleryID
					select new GalleryGroup(c)).FirstOrDefault();
		}

		public GalleryGroup GalleryGroupGetByName(string galleryTitle) {
			GalleryGroup ge = null;

			if (!string.IsNullOrEmpty(galleryTitle)) {
				ge = (from c in _db.tblGalleries
					  where c.SiteID == this.SiteID
					  && c.GalleryTitle.ToLower() == galleryTitle.ToLower()
					  select new GalleryGroup(c)).FirstOrDefault();
			}

			return ge;
		}

		public IQueryable<tblGallery> GalleryGroupListGetBySiteID() {
			return (from c in _db.tblGalleries
					where c.SiteID == this.SiteID
					select c);
		}

		public int GalleryGroupListGetBySiteIDCount() {
			return (from c in _db.tblGalleries
					where c.SiteID == this.SiteID
					select c).Count();
		}

		public GalleryMetaData GalleryMetaDataGetByFilename(string galleryImage) {
			GalleryMetaData ge = null;

			if (!string.IsNullOrEmpty(galleryImage)) {
				ge = (from c in _db.tblGalleryImageMetas
					  where c.SiteID == this.SiteID
					  && c.GalleryImage.ToLower() == galleryImage.ToLower()
					  select new GalleryMetaData(c)).FirstOrDefault();
			}

			return ge;
		}

		public GalleryMetaData GalleryMetaDataGetByID(Guid galleryImageMetaID) {
			return (from c in _db.tblGalleryImageMetas
					where c.SiteID == this.SiteID
					&& c.GalleryImageMetaID == galleryImageMetaID
					select new GalleryMetaData(c)).FirstOrDefault();
		}

		public void Dispose() {
			if (_db != null) {
				_db.Dispose();
			}
		}

		public static string ReadEmbededScript(string sResouceName) {
			string sReturn = null;

			Assembly _assembly = Assembly.GetExecutingAssembly();
			using (var stream = new StreamReader(_assembly.GetManifestResourceStream(sResouceName))) {
				sReturn = stream.ReadToEnd();
			}

			return sReturn;
		}
	}
}