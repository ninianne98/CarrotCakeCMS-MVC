using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
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

	public class SiteSkinModel {
		protected FileDataHelper helpFile = new FileDataHelper();
		private FileInfo _info = null;

		public SiteSkinModel() {
			this.CreationTime = DateTime.Now.Date;
			this.LastWriteTime = DateTime.Now.Date;
			this.SitePath = HttpContext.Current.Server.MapPath("~/");
			this.RelatedFiles = new List<FileData>();
		}

		public SiteSkinModel(string encodedPath, string altPath)
			: this(encodedPath) {
			this.AltPath = altPath.DecodeBase64();

			this.EditFile = this.AltPath;
		}

		public SiteSkinModel(string encodedPath)
			: this() {
			this.EncodedPath = encodedPath;
			this.TemplateFile = encodedPath.DecodeBase64();
			this.FullFilePath = HttpContext.Current.Server.MapPath(this.TemplateFile);

			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				this.Template = cmsHelper.Templates.Where(x => x.TemplatePath.ToLowerInvariant() == this.TemplateFile.ToLowerInvariant()).FirstOrDefault();
			}

			var ifo = new FileInfo(this.TemplateFile);
			this.TemplateFolder = ifo.Directory.FullName;

			this.EditFile = this.TemplateFile;
		}

		protected List<FileData> SetSourceFiles(string templateFileUri) {
			List<FileData> flsWorking = new List<FileData>();
			List<FileData> fldrWorking = new List<FileData>();

			List<string> lstFileExtensions = new List<string>();
			lstFileExtensions.Add(".css");
			lstFileExtensions.Add(".js");
			lstFileExtensions.Add(".cshtml");

			HttpServerUtility server = HttpContext.Current.Server;

			string templateFile = server.MapPath(templateFileUri);

			if (File.Exists(templateFile)) {
				templateFile = templateFile.Replace(@"/", @"\");

				string skinPath = templateFile.Substring(0, templateFile.LastIndexOf(@"\")).ToLowerInvariant();
				string skinName = skinPath.Substring(skinPath.LastIndexOf(@"\") + 1);

				FileData skinFolder = helpFile.GetFolderInfo("/", templateFile);
				skinFolder.FolderPath = FileDataHelper.MakeWebFolderPath(templateFile);
				fldrWorking = helpFile.SpiderDeepFoldersFD(FileDataHelper.MakeWebFolderPath(templateFile));
				fldrWorking.Add(skinFolder);

				try {
					string assetPath = String.Format("~/assets/{0}", skinName);

					if (Directory.Exists(server.MapPath(assetPath))) {
						FileData incFolder = helpFile.GetFolderInfo("/", server.MapPath(assetPath));
						fldrWorking.Add(incFolder);

						var assetFlds = helpFile.SpiderDeepFoldersFD(FileDataHelper.MakeWebFolderPath(incFolder.FolderPath));

						fldrWorking = fldrWorking.Union(assetFlds).ToList();
					}
				} catch (Exception ex) { }

				try {
					if (Directory.Exists(server.MapPath("~/includes"))) {
						FileData incFolder = helpFile.GetFolderInfo("/", server.MapPath("~/includes"));
						fldrWorking.Add(incFolder);
					}
					if (Directory.Exists(server.MapPath("~/js"))) {
						FileData incFolder = helpFile.GetFolderInfo("/", server.MapPath("~/js"));
						fldrWorking.Add(incFolder);
					}
					if (Directory.Exists(server.MapPath("~/css"))) {
						FileData incFolder = helpFile.GetFolderInfo("/", server.MapPath("~/css"));
						fldrWorking.Add(incFolder);
					}

					if (Directory.Exists(server.MapPath("~/Scripts"))) {
						FileData incFolder = helpFile.GetFolderInfo("/", server.MapPath("~/Scripts"));
						fldrWorking.Add(incFolder);
					}
					if (Directory.Exists(server.MapPath("~/Content"))) {
						FileData incFolder = helpFile.GetFolderInfo("/", server.MapPath("~/Content"));
						fldrWorking.Add(incFolder);
					}
				} catch (Exception ex) { }

				helpFile.IncludeAllFiletypes();

				foreach (FileData f in fldrWorking) {
					List<FileData> fls = helpFile.GetFiles(f.FolderPath);

					flsWorking = (from m in flsWorking.Union(fls).ToList()
								  join e in lstFileExtensions on m.FileExtension.ToLowerInvariant() equals e
								  select m).ToList();
				}

				flsWorking = flsWorking.Where(x => x.MimeType.StartsWith("text")).ToList();
			}

			if (flsWorking == null) {
				flsWorking = new List<FileData>();
			}

			return flsWorking.Distinct().OrderBy(x => x.FileName).OrderBy(x => x.FolderPath).ToList();
		}

		public void ReadFile() {
			string realPath = HttpContext.Current.Server.MapPath(this.EditFile);

			if (File.Exists(realPath)) {
				using (StreamReader sr = new StreamReader(realPath)) {
					this.FileContents = sr.ReadToEnd();
				}

				ReadRelated();
			}
		}

		public void ReadRelated() {
			string realPath = HttpContext.Current.Server.MapPath(this.EditFile);

			if (File.Exists(realPath)) {
				_info = new FileInfo(realPath);

				this.CreationTime = _info.CreationTime;
				this.LastWriteTime = _info.LastWriteTime;
				this.FullName = _info.FullName;
			}

			if (File.Exists(this.FullFilePath)) {
				this.RelatedFiles = SetSourceFiles(this.TemplateFile);
			}
		}

		public void SaveFile() {
			string realPath = HttpContext.Current.Server.MapPath(this.EditFile);

			if (File.Exists(realPath)) {
				Encoding encode = System.Text.Encoding.Default;

				using (var oWriter = new StreamWriter(realPath, false, encode)) {
					oWriter.Write(this.FileContents);
					oWriter.Close();
				}
			}
		}

		public string EncodePath(string sIn) {
			if (!(sIn.StartsWith(@"\") || sIn.StartsWith(@"/"))) {
				sIn = @"/" + sIn;
			}
			return sIn.ToLowerInvariant().EncodeBase64();
		}

		public CMSTemplate Template { get; set; }

		public List<FileData> RelatedFiles { get; set; }

		public string SitePath { get; set; }

		public string TemplateFolder { get; set; }

		public string AltPath { get; set; }

		[Required]
		[Display(Name = "Encoded Path")]
		public string EncodedPath { get; set; }

		[Required]
		[Display(Name = "Edit File")]
		public string EditFile { get; set; }

		public DateTime CreationTime { get; set; }
		public DateTime LastWriteTime { get; set; }
		public string FullName { get; set; }

		public string TemplateFile { get; set; }
		public string FullFilePath { get; set; }

		[Required]
		[Display(Name = "File Contents")]
		public string FileContents { get; set; }
	}
}