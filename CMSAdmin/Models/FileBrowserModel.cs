using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class FileBrowserModel : AjaxFileUploadModel {
		private FileDataHelper helpFile = new FileDataHelper();
		private string defaultBrowseMode = "file";

		public FileBrowserModel() {
			this.FileMsg = String.Empty;
			this.FileMsgCss = String.Empty;

			this.QueryPath = String.Empty;
			this.ViewMode = String.Empty;

			this.EscapeSpaces = true;
		}

		public FileBrowserModel(string queryPath, string useTiny, string returnVal, string viewMode)
			: this() {
			this.QueryPath = queryPath ?? @"/";
			this.ViewMode = viewMode ?? defaultBrowseMode;

			this.QueryPath = this.QueryPath.StartsWith(@"/") ? this.QueryPath : @"/" + this.QueryPath;
			this.QueryPath.Replace("//", "/").Replace("//", "/");

			if (String.IsNullOrEmpty(this.QueryPath) || this.QueryPath == @"/") {
				this.QueryPath = @"/";
				this.UpLinkVisible = false;
			} else {
				this.UpLinkVisible = true;
			}

			useTiny = useTiny ?? "0";
			returnVal = returnVal ?? "0";
			this.UseTinyMCE = useTiny == "1" || useTiny.ToLower() == "true";
			this.ReturnMode = returnVal == "1" || returnVal.ToLower() == "true";
			this.Thumbnails = this.ViewMode.ToLower() != defaultBrowseMode;

			if (this.UpLinkVisible) {
				string sUrlUp = this.QueryPath.Substring(0, this.QueryPath.Substring(0, this.QueryPath.Length - 2).LastIndexOf('/')) + @"/";
				this.UpLink = String.Format("{0}?fldrpath={1}&useTiny={2}&returnvalue={3}&viewmode={4}", SiteData.CurrentScriptName, HttpUtility.UrlEncode(sUrlUp), this.UseTinyMCE, this.ReturnMode, this.ViewMode);
			}

			this.ThumbViewLink = String.Format("{0}?fldrpath={1}&useTiny={2}&returnvalue={3}&viewmode=thumb", SiteData.CurrentScriptName, HttpUtility.UrlEncode(this.QueryPath), this.UseTinyMCE, this.ViewMode);
			this.FileViewLink = String.Format("{0}?fldrpath={1}&useTiny={2}&returnvalue={3}&viewmode=file", SiteData.CurrentScriptName, HttpUtility.UrlEncode(this.QueryPath), this.UseTinyMCE, this.ViewMode);

			this.Dirs = helpFile.GetFolders(this.QueryPath);
			this.Files = helpFile.GetFiles(this.QueryPath);

			if (this.Thumbnails) {
				this.Files = this.Files.Where(x => x.MimeType.StartsWith("image/")).ToList();
			}
		}

		public List<FileData> Files { get; set; }
		public List<FileData> Dirs { get; set; }

		[DataType(DataType.Upload)]
		[Display(Name = "Posted File")]
		public HttpPostedFileBase PostedFile { get; set; }

		public string FileMsg { get; set; }

		public string FileMsgCss { get; set; }

		public bool UseTinyMCE { get; set; }

		public bool ReturnMode { get; set; }

		public bool Thumbnails { get; set; }

		public string QueryMode { get; set; }

		public string ViewMode { get; set; }

		public bool UpLinkVisible { get; set; }

		public string UpLink { get; set; }

		public string ThumbViewLink { get; set; }

		public string FileViewLink { get; set; }

		public string FileImageLink(string sMime) {
			sMime = sMime.ToLower();
			var mime = sMime.Substring(0, sMime.IndexOf("/"));

			string sImage = "plain";

			switch (mime) {
				case "image":
					sImage = "image";
					break;

				case "audio":
					sImage = "audio";
					break;

				case "video":
					sImage = "video";
					break;

				case "application":
					sImage = "application";
					break;

				default:
					sImage = "plain";
					break;
			}

			switch (sMime) {
				case "application/pdf":
					sImage = "pdf";
					break;

				case "text/html":
				case "text/asp":
				case "text/aspx":
					sImage = "html";
					break;

				case "application/excel":
					sImage = "spreadsheet";
					break;

				case "application/rtf":
				case "application/msword":
					sImage = "wordprocessing";
					break;

				case "application/x-compressed":
				case "application/zip":
					sImage = "compress";
					break;
			}

			return sImage;
		}

		public string CreateFileLink(string sPath) {
			return String.Format("javascript:SetFile('{0}');", sPath);
		}

		public string CreateFileSrc(string sPath, string sFile, string sMime) {
			if (this.FileImageLink(sMime).ToLower() == "image") {
				return String.Format("{0}{1}", sPath, sFile).ToLower();
			} else {
				return "/Assets/Admin/images/document.png";
			}
		}

		public void RemoveFiles() {
			var sPath = SetSitePath(this.QueryPath);
			List<FileData> selectedDelete = this.Files.Where(x => x.SelectedItem).Select(x => x).ToList();

			foreach (var f in selectedDelete) {
				if (f.SelectedItem) {
					File.Delete(Path.Combine(sPath, f.FileName));
				}
			}
		}

		public void UploadFile() {
			this.FileMsg = String.Empty;
			this.FileMsgCss = String.Empty;

			try {
				if (this.PostedFile != null) {
					var sPath = SetSitePath(this.QueryPath);
					string uploadedFileName = this.PostedFile.FileName;

					if (!(from b in helpFile.BlockedTypes
						  where uploadedFileName.ToLower().Contains("." + b.ToLower())
						  select b).Any()) {
						if (this.EscapeSpaces) {
							uploadedFileName = uploadedFileName.Replace(" ", "-");
						}

						this.PostedFile.SaveAs(Path.Combine(sPath, uploadedFileName));

						this.FileMsg = String.Format("file [{0}] uploaded!", uploadedFileName);
						this.FileMsgCss = "uploadSuccess";
					} else {
						this.FileMsg = String.Format("[{0}] is a blocked filetype.", uploadedFileName);
						this.FileMsgCss = "uploadBlocked";
					}
				} else {
					this.FileMsg = "No file detected for upload.";
					this.FileMsgCss = "uploadNoFile";
				}
			} catch (Exception ex) {
				this.FileMsg = ex.ToString();
				this.FileMsgCss = "uploadFail";
			}
		}
	}

	//==================

	public class AjaxFileUploadModel {
		protected FileDataHelper helpFile = new FileDataHelper();

		public AjaxFileUploadModel() { }

		public string QueryPath { get; set; }

		[Display(Name = "Change spaces to dashes")]
		public bool EscapeSpaces { get; set; }

		protected string SetSitePath(string sPath) {
			return FileDataHelper.MakeFileFolderPath(sPath);
		}

		[DataType(DataType.Upload)]
		[Display(Name = "Posted Files")]
		public IEnumerable<HttpPostedFileBase> PostedFiles { get; set; }

		public string UploadFile(HttpPostedFileBase postedFile) {
			try {
				if (postedFile != null) {
					var sPath = SetSitePath(this.QueryPath);
					string uploadedFileName = postedFile.FileName;

					if (!(from b in helpFile.BlockedTypes
						  where uploadedFileName.ToLower().Contains("." + b.ToLower())
						  select b).Any()) {
						if (this.EscapeSpaces) {
							uploadedFileName = uploadedFileName.Replace(" ", "-");
							uploadedFileName = uploadedFileName.Replace("+", "-");
							uploadedFileName = uploadedFileName.Replace("%20", "-");
						}

						postedFile.SaveAs(Path.Combine(sPath, uploadedFileName));

						return uploadedFileName;
					} else {
						throw new Exception("Blocked File Type");
					}
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("fileupload", ex);
				throw ex;
			}

			return String.Empty;
		}
	}
}