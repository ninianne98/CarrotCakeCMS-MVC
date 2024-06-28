using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CarrotCake.CMS.Plugins.PhotoGallery.Models {

	public class EditPhotoGalleryModel {
		protected FileDataHelper fileHelper = new FileDataHelper();

		public EditPhotoGalleryModel() {
			this.Date = DateTime.Now.Date;
			this.SelectedFolder = @"/";

			BuildFolderList();
		}

		public EditPhotoGalleryModel(Guid siteId, Guid galleryId)
			: this() {
			this.SiteID = siteId;
			this.GalleryID = galleryId;

			LoadLists();
		}

		public EditPhotoGalleryModel(Guid siteId, Guid galleryId, string path)
				: this() {
			this.SiteID = siteId;
			this.GalleryID = galleryId;
			this.SelectedFolder = path;
			this.RestrictFolder = !string.IsNullOrEmpty(this.SelectedFolder);

			LoadLists();
		}

		public DateTime? Date { get; set; }
		public bool RestrictDate { get; set; }

		public bool RestrictFolder { get; set; }

		public bool SaveGallery { get; set; }

		public Guid SiteID { get; set; }

		public Guid GalleryID { get; set; }

		public GalleryGroup Gallery { get; set; }

		public List<FileData> ImageFiles { get; set; } = new List<FileData>();
		public List<FileData> SiteImages { get; set; } = new List<FileData>();

		public string SelectedFolder { get; set; } = "/";
		public List<FileData> FileFolders { get; set; } = new List<FileData>();

		public string GalleryOrder { get; set; } = string.Empty;

		public string SetSitePath(string sPath) {
			return FileDataHelper.MakeFileFolderPath(sPath);
		}

		protected void BuildFolderList() {
			List<FileData> lstFolders = new List<FileData>();

			string sRoot = HttpContext.Current.Server.MapPath("~/");

			string[] subdirs;
			try {
				subdirs = Directory.GetDirectories(sRoot);
			} catch {
				subdirs = null;
			}

			var now = DateTime.Now.Date;

			lstFolders.Add(new FileData { FileName = "[Entire Site]", FolderPath = "/", FileDate = now });
			lstFolders.Add(new FileData { FileName = "/images/", FolderPath = "/images/", FileDate = now });

			if (subdirs != null) {
				foreach (string theDir in subdirs) {
					string w = FileDataHelper.MakeWebFolderPath(theDir);
					lstFolders.Add(new FileData { FileName = w, FolderPath = w, FileDate = now });

					string[] subdirs2;
					try {
						subdirs2 = Directory.GetDirectories(theDir);
					} catch {
						subdirs2 = null;
					}

					if (subdirs2 != null) {
						foreach (string theDir2 in subdirs2) {
							string w2 = FileDataHelper.MakeWebFolderPath(theDir2);
							lstFolders.Add(new FileData { FileName = w2, FolderPath = w2, FileDate = now });
						}
					}
				}
			}

			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/app_code/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/app_data/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/app_start/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/bin/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/obj/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/views/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/controllers/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/."));

			this.FileFolders = lstFolders.Distinct().OrderBy(f => f.FileName).ToList();
		}

		public void LoadGallery() {
			using (var gh = new GalleryHelper(this.SiteID)) {
				this.Gallery = gh.GalleryGroupGetByID(this.GalleryID);
			}
		}

		protected void LoadLists() {
			LoadGallery();

			var gal = this.Gallery;

			if (gal != null) {
				this.ImageFiles = (from g in gal.GalleryImages
								   orderby g.ImageOrder ascending
								   select fileHelper.GetFileInfo(g.GalleryImage, g.GalleryImage)).ToList();
			}

			SetSourceFiles(null, this.SelectedFolder);
		}

		protected Dictionary<int, string> ParseGalleryImages() {
			string imageList = this.GalleryOrder ?? string.Empty;
			var lstImages = new Dictionary<int, string>();

			if (!string.IsNullOrEmpty(this.GalleryOrder)) {
				imageList = imageList.Replace("\r\n", "\n");
				imageList = imageList.Replace("\r", "\n");
				var arrImageRows = imageList.Split('\n');

				int row = 0;
				foreach (string arrImgCell in arrImageRows) {
					if (!string.IsNullOrEmpty(arrImgCell)) {
						var w = arrImgCell.Split('\t');
						var img = w[1];
						if (!string.IsNullOrEmpty(img)) {
							lstImages.Add(row, img);
						}
					}
					row++;
				}
			}

			return lstImages;
		}

		public void SetSrcFiles() {
			Dictionary<int, string> lstImages = ParseGalleryImages();

			DateTime? dtFilter = null;

			if (this.RestrictDate) {
				dtFilter = DateTime.Now.Date;
				try {
					dtFilter = this.Date;
				} catch { }
			}

			this.ImageFiles = (from g in lstImages
							   orderby g.Key ascending
							   select fileHelper.GetFileInfo(g.Value, g.Value)).ToList();

			string sPath = "/";

			if (this.RestrictFolder) {
				sPath = this.SelectedFolder;
			}

			SetSourceFiles(dtFilter, sPath);
		}

		public void SetSourceFiles(DateTime? dtFilter, string path) {
			List<FileData> flsWorking = new List<FileData>();
			List<FileData> fldrWorking = new List<FileData>();

			fldrWorking = fileHelper.SpiderDeepFoldersFD(path);

			if (Directory.Exists(FileDataHelper.MakeFileFolderPath(path))) {
				var fls = fileHelper.GetFiles(path);

				var imgs = (from m in flsWorking.Union(fls).ToList()
							where m.MimeType.StartsWith("image")
							select m).ToList();

				flsWorking = flsWorking.Union(imgs).ToList();
			}

			foreach (var f in fldrWorking) {
				var fls = fileHelper.GetFiles(f.FolderPath);

				var imgs = (from m in flsWorking.Union(fls).ToList()
							where m.MimeType.StartsWith("image")
							select m).ToList();

				flsWorking = flsWorking.Union(imgs).ToList();
			}

			flsWorking = flsWorking.Where(x => x.MimeType.StartsWith("image")).ToList();

			if (dtFilter.HasValue) {
				DateTime dtFlt = dtFilter.Value;
				flsWorking = flsWorking.Where(x => x.FileDate >= dtFlt.AddDays(-14) && x.FileDate <= dtFlt.AddDays(14)).ToList();
			}

			this.SiteImages = flsWorking.Distinct().OrderBy(x => x.FileName).OrderBy(x => x.FolderPath).ToList();
		}

		public void Save() {
			using (var gh = new GalleryHelper(this.SiteID)) {
				Dictionary<int, string> images = ParseGalleryImages();

				if (images != null) {
					int pos = 0;
					foreach (var img in images) {
						if (!string.IsNullOrEmpty(img.Value)) {
							var theImg = gh.GalleryImageEntryGetByFilename(this.GalleryID, img.Value);

							if (theImg == null) {
								theImg = new GalleryImageEntry();
								theImg.GalleryImage = img.Value;
								theImg.GalleryImageID = Guid.NewGuid();
								theImg.GalleryID = this.GalleryID;
							}

							theImg.ImageOrder = pos;

							theImg.Save();
						}

						pos++;
					}

					List<string> lst = (from l in images select l.Value.ToLower()).ToList();

					gh.GalleryImageCleanup(this.GalleryID, lst);
				}
			}
		}
	}
}