﻿using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
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
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class FileBrowserModel : AjaxFileUploadModel {
		private string defaultBrowseMode = "file";

		public FileBrowserModel() {
			this.FileMsg = string.Empty;
			this.FileMsgCss = string.Empty;

			this.QueryPath = string.Empty;
			this.ViewMode = string.Empty;

			this.EscapeSpaces = true;
		}

		public FileBrowserModel(string queryPath, string useTiny, string returnVal, string viewMode)
			: this() {
			this.QueryPath = queryPath ?? @"/";
			this.ViewMode = viewMode ?? defaultBrowseMode;

			this.QueryPath = this.QueryPath.StartsWith(@"/") ? this.QueryPath : @"/" + this.QueryPath;
			this.QueryPath.Replace("//", "/").Replace("//", "/");

			if (string.IsNullOrEmpty(this.QueryPath) || this.QueryPath == @"/") {
				this.QueryPath = @"/";
				this.UpLinkVisible = false;
			} else {
				this.UpLinkVisible = true;
			}

			useTiny = useTiny ?? "0";
			returnVal = returnVal ?? "0";
			this.UseTinyMCE = useTiny == "1" || useTiny.ToLowerInvariant() == "true";
			this.ReturnMode = returnVal == "1" || returnVal.ToLowerInvariant() == "true";
			this.Thumbnails = this.ViewMode.ToLowerInvariant() != defaultBrowseMode;

			string linkPatt = "{0}?fldrpath={1}&useTiny={2}&returnvalue={3}&viewmode={4}";

			if (this.UpLinkVisible) {
				string sUrlUp = this.QueryPath.Substring(0, this.QueryPath.Substring(0, this.QueryPath.Length - 2).LastIndexOf('/')) + @"/";
				this.UpLink = string.Format(linkPatt, SiteData.CurrentScriptName, HttpUtility.UrlEncode(sUrlUp), this.UseTinyMCE, this.ReturnMode, this.ViewMode);
			}

			this.ThumbViewLink = string.Format(linkPatt, SiteData.CurrentScriptName, HttpUtility.UrlEncode(this.QueryPath), this.UseTinyMCE, this.ReturnMode, "thumb");
			this.FileViewLink = string.Format(linkPatt, SiteData.CurrentScriptName, HttpUtility.UrlEncode(this.QueryPath), this.UseTinyMCE, this.ReturnMode, "file");

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
			sMime = sMime.ToLowerInvariant();
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
			return string.Format("javascript:SetFile('{0}');", sPath);
		}

		public string CreateFileSrc(string sPath, string sFile, string sMime) {
			if (this.FileImageLink(sMime).ToLowerInvariant() == "image") {
				return string.Format("{0}{1}", sPath, sFile).ToLowerInvariant();
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
			this.FileMsg = string.Empty;
			this.FileMsgCss = string.Empty;

			try {
				if (this.PostedFile != null) {
					var sPath = SetSitePath(this.QueryPath);
					string uploadedFileName = this.PostedFile.FileName;

					if (!(from b in helpFile.BlockedTypes
						  where uploadedFileName.ToLowerInvariant().Contains("." + b.ToLowerInvariant())
						  select b).Any()) {
						if (this.EscapeSpaces) {
							uploadedFileName = uploadedFileName.Replace(" ", "-");
						}

						this.PostedFile.SaveAs(Path.Combine(sPath, uploadedFileName));

						this.FileMsg = string.Format("file [{0}] uploaded!", uploadedFileName);
						this.FileMsgCss = "uploadSuccess";
					} else {
						this.FileMsg = string.Format("[{0}] is a blocked filetype.", uploadedFileName);
						this.FileMsgCss = "uploadBlocked";
					}
				} else {
					this.FileMsg = "No file detected for upload.";
					this.FileMsgCss = "uploadNoFile";
				}
			} catch (Exception ex) {
				this.FileMsg = ex.ToString();
				this.FileMsgCss = "uploadFail";
				SiteData.WriteDebugException("uploadfile", ex);
			}
		}
	}

	//==================

	public class AjaxFileUploadModel {
		protected FileDataHelper helpFile = CMSConfigHelper.GetFileDataHelper();

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
						  where uploadedFileName.ToLowerInvariant().EndsWith(string.Format(".{0}", b).ToLowerInvariant())
						  select b).Any()) {
						if (this.EscapeSpaces) {
							uploadedFileName = uploadedFileName.Replace(" ", "-");
							uploadedFileName = uploadedFileName.Replace("_", "-");
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
				SiteData.WriteDebugException("ajax-fileupload", ex);
				throw;
			}

			return string.Empty;
		}
	}
}