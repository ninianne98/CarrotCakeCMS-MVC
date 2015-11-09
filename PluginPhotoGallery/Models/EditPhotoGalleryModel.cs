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

		public DateTime? Date { get; set; }
		public bool RestrictDate { get; set; }

		public bool RestrictFolder { get; set; }

		public bool SaveGallery { get; set; }

		public Guid SiteID { get; set; }

		public Guid GalleryID { get; set; }

		public GalleryGroup Gallery { get; set; }

		public List<FileData> ImageFiles { get; set; }
		public List<FileData> SiteImages { get; set; }

		public string SelectedFolder { get; set; }
		public List<FileData> FileFolders { get; set; }

		public string GalleryOrder { get; set; }

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

			lstFolders.Add(new FileData { FileName = "[Entire Site]", FolderPath = "/", FileDate = DateTime.Now });

			if (subdirs != null) {
				foreach (string theDir in subdirs) {
					string w = FileDataHelper.MakeWebFolderPath(theDir);
					lstFolders.Add(new FileData { FileName = w, FolderPath = w, FileDate = DateTime.Now });

					string[] subdirs2;
					try {
						subdirs2 = Directory.GetDirectories(theDir);
					} catch {
						subdirs2 = null;
					}

					if (subdirs2 != null) {
						foreach (string theDir2 in subdirs2) {
							string w2 = FileDataHelper.MakeWebFolderPath(theDir2);
							lstFolders.Add(new FileData { FileName = w2, FolderPath = w2, FileDate = DateTime.Now });
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

			this.FileFolders = lstFolders.OrderBy(f => f.FileName).ToList();
		}

		public void LoadGallery() {
			GalleryHelper gh = new GalleryHelper(this.SiteID);
			this.Gallery = gh.GalleryGroupGetByID(this.GalleryID);
		}

		protected void LoadLists() {
			LoadGallery();

			var gal = this.Gallery;

			if (gal != null) {
				this.ImageFiles = (from g in gal.GalleryImages
								   orderby g.ImageOrder ascending
								   select fileHelper.GetFileInfo(g.GalleryImage, g.GalleryImage)).ToList();
			}

			SetSourceFiles(null, "/");
		}

		protected Dictionary<int, string> ParseGalleryImages() {
			string sImageList = this.GalleryOrder ?? String.Empty;
			Dictionary<int, string> lstImages = new Dictionary<int, string>();

			if (!String.IsNullOrEmpty(this.GalleryOrder)) {
				sImageList = sImageList.Replace("\r\n", "\n");
				sImageList = sImageList.Replace("\r", "\n");
				var arrImageRows = sImageList.Split('\n');

				int iRow = 0;
				foreach (string arrImgCell in arrImageRows) {
					if (!string.IsNullOrEmpty(arrImgCell)) {
						var w = arrImgCell.Split('\t');
						var img = w[1];
						if (!string.IsNullOrEmpty(img)) {
							lstImages.Add(iRow, img);
						}
					}
					iRow++;
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

		public void SetSourceFiles(DateTime? dtFilter, string sPath) {
			List<FileData> flsWorking = new List<FileData>();
			List<FileData> fldrWorking = new List<FileData>();

			fldrWorking = fileHelper.SpiderDeepFoldersFD(sPath);

			if (Directory.Exists(FileDataHelper.MakeFileFolderPath(sPath))) {
				var fls = fileHelper.GetFiles(sPath);

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

			this.SiteImages = flsWorking.OrderBy(x => x.FileName).OrderBy(x => x.FolderPath).ToList();
		}

		public void Save() {
			GalleryHelper gh = new GalleryHelper(this.SiteID);

			Dictionary<int, string> lstImages = ParseGalleryImages();
			int iPos = 0;

			foreach (var img in lstImages) {
				if (!string.IsNullOrEmpty(img.Value)) {
					var theImg = gh.GalleryImageEntryGetByFilename(this.GalleryID, img.Value);

					if (theImg == null) {
						theImg = new GalleryImageEntry();
						theImg.GalleryImage = img.Value;
						theImg.GalleryImageID = Guid.NewGuid();
						theImg.GalleryID = this.GalleryID;
					}

					theImg.ImageOrder = iPos;

					theImg.Save();
				}

				iPos++;

				List<string> lst = (from l in lstImages
									select l.Value.ToLower()).ToList();

				gh.GalleryImageCleanup(this.GalleryID, lst);
			}
		}
	}
}