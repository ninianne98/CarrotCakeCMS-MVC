using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace Carrotware.Web.UI.Components {

	public class FileData {

		public FileData() {
			this.FolderPath = string.Empty;
			this.FileName = "unknown";
			this.FileSize = 0;
			this.FileExtension = string.Empty;
			this.FileSizeFriendly = "0B";
			this.FileDate = Convert.ToDateTime("1900-01-01");
			this.MimeType = "x-application/octet-stream";
		}

		public string FileName { get; set; }
		public string FileExtension { get; set; }
		public DateTime FileDate { get; set; }
		public long FileSize { get; set; }
		public string FileSizeFriendly { get; set; }
		public string FolderPath { get; set; }
		public string MimeType { get; set; }
		public bool SelectedItem { get; set; }

		public string FullFileName {
			get {
				return string.Format("/{0}/{1}", this.FolderPath, this.FileName).FixPathSlashes();
			}
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;

			if (obj is FileData) {
				FileData p = (FileData)obj;
				return (string.Format("{0}", this.FullFileName).ToLowerInvariant() == string.Format("{0}", p.FullFileName).ToLowerInvariant());
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return string.Format("{0}", this.FullFileName).ToLowerInvariant().GetHashCode();
		}
	}

	//=================================================
	public class FileDataHelper {

		public FileDataHelper() { }

		public FileDataHelper(string blockedExts) {
			_blockedTypes = blockedExts;
		}

		private static string _wwwpath = null;

		private static string WebPath {
			get {
				if (_wwwpath == null) {
					_wwwpath = HttpContext.Current.Server.MapPath("~/").NormalizeFilename();
				}
				return _wwwpath;
			}
		}

		private string _blockedTypes = null;

		public List<string> BlockedTypes {
			get {
				if (_blockedTypes == null) {
					_blockedTypes = "asp;aspx;ascx;asmx;svc;asax;axd;ashx;dll;pdb;exe;cs;vb;cshtml;vbhtml;master;config;xml;user;csproj;vbproj;sln";
				}
				return _blockedTypes.Split(';').ToList();
			}
		}

		public void IncludeAllFiletypes() {
			_blockedTypes = string.Empty;
		}

		public FileData GetFolderInfo(string sQuery, string myFile) {
			string sPath = MakeFileFolderPath(sQuery);

			string myFileName;

			var f = new FileData();
			f.FileName = myFile;

			bool isFolder = Directory.Exists(myFile);

			if (isFolder) {
				myFileName = myFile;
				f.FileName = Path.GetFileName(myFileName).Trim();
				if (myFile.Length >= sPath.Length) {
					f.FolderPath = string.Format("/{0}/{1}/", sQuery, myFile.Substring(sPath.Length)).FixFolderSlashes();
				}
				f.FileDate = Directory.GetLastWriteTime(myFile);
			} else {
				myFileName = Path.GetFileName(myFile).Trim();

				if (myFileName.Length > 0) {
					var fileInfo = new FileInfo(Path.Combine(sPath, myFileName));
					string path = (sQuery + "/" + myFileName).FixPathSlashes();

					f.FileName = myFileName;
					f.FolderPath = path;
					f.FileDate = fileInfo.LastWriteTime;
				}
			}

			return f;
		}

		public List<FileData> GetFolders(string sQuery) {
			string sPath = MakeFileFolderPath(sQuery);

			var files = new List<FileData>();

			if (Directory.Exists(sPath)) {
				foreach (string myFile in Directory.GetDirectories(sPath, "*.*")) {
					string myFileName;
					var f = new FileData();
					f.FileName = myFile;
					myFileName = Path.GetFileName(myFile).Trim();
					if (myFileName.Length > 0) {
						f = GetFolderInfo(sQuery, myFile);
						files.Add(f);
					}
				}
			}

			return files;
		}

		public FileData GetFileInfo(string sQuery, string myFile) {
			string sPath = MakeFileFolderPath(sQuery).NormalizeFilename();

			if (!string.IsNullOrEmpty(sQuery) && !string.IsNullOrEmpty(myFile)
						&& sQuery.ToLowerInvariant() == myFile.ToLowerInvariant()) {
				var fileInfo = new FileInfo((_wwwpath + "/" + myFile).NormalizeFilename());

				sQuery = (fileInfo.DirectoryName ?? string.Empty).NormalizeFilename();
				myFile = fileInfo.Name;
				sPath = sQuery;
			}

			sQuery = sQuery.NormalizeFilename();

			string myFileName = Path.GetFileName(myFile).Trim();
			DateTime myFileDate = Convert.ToDateTime("1899-01-01");
			string myFileSizeF = string.Empty;
			long myFileSize;

			var f = new FileData();
			f.FileName = myFile;

			var testFile = Path.Combine(sPath, myFileName).NormalizeFilename();

			if (myFileName.Length > 0 && File.Exists(testFile)) {
				var fileInfo = new FileInfo(testFile);
				myFileDate = fileInfo.LastWriteTime;
				myFileSize = fileInfo.Length;

				myFileSizeF = myFileSize.ToString() + " B";

				if (myFileSize > 1500) {
					if (myFileSize > (1024 * 1024)) {
						myFileSizeF = (Convert.ToDouble(Convert.ToInt32((myFileSize * 100) / (1024 * 1024))) / 100).ToString() + " MB";
					} else {
						myFileSizeF = (Convert.ToDouble(Convert.ToInt32((myFileSize * 100) / 1024)) / 100).ToString() + " KB";
					}
				}
				string myPath = sQuery.FixPathSlashes();

				f.FileName = Path.GetFileName(myFileName);
				f.FolderPath = MakeWebFolderPath(myPath);
				f.FileDate = myFileDate;
				f.FileSize = myFileSize;
				f.FileSizeFriendly = myFileSizeF;
				if (!string.IsNullOrEmpty(fileInfo.Extension)) {
					f.FileExtension = fileInfo.Extension.ToLowerInvariant();
				} else {
					f.FileExtension = ".";
				}

				f.MimeType = "text/plain";

				try {
					if ((from b in MimeTypes
						 where b.Key.ToLowerInvariant() == f.FileExtension.ToLowerInvariant()
						 select b).Any()) {
						f.MimeType = (from b in MimeTypes
									  where b.Key.ToLowerInvariant() == f.FileExtension.ToLowerInvariant()
									  select b.Value).FirstOrDefault();
					}
				} catch (Exception ex) { }
			}

			return f;
		}

		public static string MakeFilePathUniform(string sDirPath) {
			string _path = "/";
			if (!string.IsNullOrEmpty(sDirPath)) {
				_path = sDirPath.FixPathSlashes();
				_path = _path + @"/";
			}
			return _path;
		}

		public static string MakeFileFolderPath(string sDirPath) {
			string _path = MakeFilePathUniform(sDirPath);
			string _map = HttpContext.Current.Server.MapPath(_path);

			return _map;
		}

		public static string MakeWebFolderPath(string sDirPath) {
			string sPathPrefix = "/";

			if (!string.IsNullOrEmpty(sDirPath)) {
				sDirPath = sDirPath.NormalizeFilename();
				sPathPrefix = sDirPath.Replace(WebPath, @"/");
			}
			sPathPrefix = MakeFilePathUniform(sPathPrefix);

			return sPathPrefix;
		}

		public List<FileData> GetFiles(string sQuery) {
			string sPath = MakeFileFolderPath(sQuery);

			var files = new List<FileData>();

			if (Directory.Exists(sPath)) {
				foreach (string myFile in Directory.GetFiles(sPath, "*.*")) {
					string myFileName = Path.GetFileName(myFile).Trim();

					var f = new FileData();
					f.FileName = myFileName;

					if (myFileName.Length > 0) {
						f = GetFileInfo(sQuery, myFile);

						try {
							if (!(from b in this.BlockedTypes
								  where b.ToLowerInvariant().Replace(".", "") == f.FileExtension.Replace(".", "")
								  select b).Any()) {
								files.Add(f);
							}
						} catch (Exception ex) { }
					}
				}
			}

			return files;
		}

		private List<string> _spiderdirs = null;

		private void SpiderFolders(string sPath) {
			string[] subdirs;
			try {
				if (Directory.Exists(sPath)) {
					subdirs = Directory.GetDirectories(sPath);
				} else {
					subdirs = null;
				}
			} catch {
				subdirs = null;
			}

			if (subdirs != null) {
				foreach (string theDir in subdirs) {
					_spiderdirs.Add(theDir);
					SpiderFolders(theDir);
				}
			}
		}

		public List<string> SpiderDeepFolders(string sQuery) {
			string sPath = MakeFileFolderPath(sQuery);

			_spiderdirs = new List<string>();

			SpiderFolders(sPath);

			return _spiderdirs;
		}

		private List<FileData> _spiderFD = null;

		private void SpiderFoldersFD(string sQuery) {
			string sPath = MakeFileFolderPath(sQuery);

			string[] subdirs;
			try {
				if (Directory.Exists(sPath)) {
					subdirs = Directory.GetDirectories(sPath);
				} else {
					subdirs = null;
				}
			} catch {
				subdirs = null;
			}

			if (subdirs != null) {
				foreach (string theDir in subdirs) {
					var f = GetFolderInfo(sQuery, theDir);
					_spiderFD.Add(f);
					SpiderFoldersFD(f.FolderPath);
				}
			}
		}

		public List<FileData> SpiderDeepFoldersFD(string sQuery) {
			_spiderFD = new List<FileData>();

			string sPath = MakeFileFolderPath(sQuery);

			SpiderFoldersFD(sQuery);

			return _spiderFD;
		}

		private static ConcurrentDictionary<string, string> _dict = null;

		public static Dictionary<string, string> MimeTypes {
			get {
				if (_dict == null) {
					_dict = new ConcurrentDictionary<string, string>();
					_dict.TryAdd(".7z", "application/octet-stream");
					_dict.TryAdd(".ai", "application/postscript");
					_dict.TryAdd(".aif", "audio/aiff");
					_dict.TryAdd(".aifc", "audio/aiff");
					_dict.TryAdd(".aiff", "audio/aiff");
					_dict.TryAdd(".aps", "application/mime");
					_dict.TryAdd(".arc", "application/octet-stream");
					_dict.TryAdd(".arj", "application/octet-stream");
					_dict.TryAdd(".asa", "text/asp");
					_dict.TryAdd(".asax", "text/aspx");
					_dict.TryAdd(".ascx", "text/aspx");
					_dict.TryAdd(".asf", "video/x-ms-asf");
					_dict.TryAdd(".asm", "text/x-asm");
					_dict.TryAdd(".asmx", "text/aspx");
					_dict.TryAdd(".asp", "text/asp");
					_dict.TryAdd(".aspx", "text/aspx");
					_dict.TryAdd(".asx", "video/x-ms-asf");
					_dict.TryAdd(".au", "audio/basic");
					_dict.TryAdd(".avi", "video/avi");
					_dict.TryAdd(".avs", "video/avs-video");
					_dict.TryAdd(".bin", "application/octet-stream");
					_dict.TryAdd(".bmp", "image/bmp");
					_dict.TryAdd(".bz", "application/x-bzip");
					_dict.TryAdd(".bz2", "application/x-bzip2");
					_dict.TryAdd(".c", "text/plain");
					_dict.TryAdd(".c++", "text/plain");
					_dict.TryAdd(".cc", "text/plain");
					_dict.TryAdd(".cer", "application/x-x509-ca-cert");
					_dict.TryAdd(".class", "application/java");
					_dict.TryAdd(".com", "application/octet-stream");
					_dict.TryAdd(".conf", "text/plain");
					_dict.TryAdd(".config", "text/aspx");
					_dict.TryAdd(".cpp", "text/x-c");
					_dict.TryAdd(".crt", "application/x-x509-ca-cert");
					_dict.TryAdd(".csh", "application/x-csh");
					_dict.TryAdd(".cshtml", "text/aspx");
					_dict.TryAdd(".css", "text/css");
					_dict.TryAdd(".def", "text/plain");
					_dict.TryAdd(".dir", "application/x-director");
					_dict.TryAdd(".doc", "application/msword");
					_dict.TryAdd(".docx", "application/msword");
					_dict.TryAdd(".dot", "application/msword");
					_dict.TryAdd(".dump", "application/octet-stream");
					_dict.TryAdd(".dvi", "application/x-dvi");
					_dict.TryAdd(".dwf", "model/vnd.dwf");
					_dict.TryAdd(".dwg", "application/acad");
					_dict.TryAdd(".dxf", "application/dxf");
					_dict.TryAdd(".el", "text/x-script.elisp");
					_dict.TryAdd(".eot", "font/eot");
					_dict.TryAdd(".eps", "application/postscript");
					_dict.TryAdd(".es", "application/x-esrehber");
					_dict.TryAdd(".etx", "text/x-setext");
					_dict.TryAdd(".evy", "application/envoy");
					_dict.TryAdd(".exe", "application/octet-stream");
					_dict.TryAdd(".f", "text/plain");
					_dict.TryAdd(".fif", "image/fif");
					_dict.TryAdd(".fli", "video/x-fli");
					_dict.TryAdd(".flo", "image/florian");
					_dict.TryAdd(".flx", "text/vnd.fmi.flexstor");
					_dict.TryAdd(".fmf", "video/x-atomic3d-feature");
					_dict.TryAdd(".for", "text/plain");
					_dict.TryAdd(".frl", "application/freeloader");
					_dict.TryAdd(".gif", "image/gif");
					_dict.TryAdd(".gl", "video/gl");
					_dict.TryAdd(".gsd", "audio/x-gsm");
					_dict.TryAdd(".gsm", "audio/x-gsm");
					_dict.TryAdd(".gsp", "application/x-gsp");
					_dict.TryAdd(".gss", "application/x-gss");
					_dict.TryAdd(".gtar", "application/x-gtar");
					_dict.TryAdd(".gz", "application/x-compressed");
					_dict.TryAdd(".gzip", "application/x-compressed");
					_dict.TryAdd(".h", "text/plain");
					_dict.TryAdd(".help", "application/x-helpfile");
					_dict.TryAdd(".hh", "text/plain");
					_dict.TryAdd(".hlp", "application/hlp");
					_dict.TryAdd(".hpg", "application/vnd.hp-hpgl");
					_dict.TryAdd(".hpgl", "application/vnd.hp-hpgl");
					_dict.TryAdd(".hqx", "application/binhex");
					_dict.TryAdd(".hta", "application/hta");
					_dict.TryAdd(".htc", "text/x-component");
					_dict.TryAdd(".htm", "text/html");
					_dict.TryAdd(".html", "text/html");
					_dict.TryAdd(".htmls", "text/html");
					_dict.TryAdd(".htt", "text/webviewhtml");
					_dict.TryAdd(".htx", "text/html");
					_dict.TryAdd(".ico", "image/x-icon");
					_dict.TryAdd(".imap", "application/x-httpd-imap");
					_dict.TryAdd(".inf", "application/inf");
					_dict.TryAdd(".ini", "text/plain");
					_dict.TryAdd(".it", "audio/it");
					_dict.TryAdd(".java", "text/plain");
					_dict.TryAdd(".jpe", "image/jpeg");
					_dict.TryAdd(".jpeg", "image/jpeg");
					_dict.TryAdd(".jpg", "image/jpeg");
					_dict.TryAdd(".js", "text/javascript");
					_dict.TryAdd(".log", "text/plain");
					_dict.TryAdd(".m", "text/plain");
					_dict.TryAdd(".m1v", "video/mpeg");
					_dict.TryAdd(".m2a", "audio/mpeg");
					_dict.TryAdd(".m2v", "video/mpeg");
					_dict.TryAdd(".m3u", "audio/x-mpequrl");
					_dict.TryAdd(".man", "application/x-troff-man");
					_dict.TryAdd(".map", "application/x-navimap");
					_dict.TryAdd(".mcd", "application/mcad");
					_dict.TryAdd(".md", "text/plain");
					_dict.TryAdd(".mht", "message/rfc822");
					_dict.TryAdd(".mhtml", "message/rfc822");
					_dict.TryAdd(".mid", "audio/midi");
					_dict.TryAdd(".midi", "audio/midi");
					_dict.TryAdd(".mime", "message/rfc822");
					_dict.TryAdd(".mkv", "video/webm");
					_dict.TryAdd(".mm", "application/base64");
					_dict.TryAdd(".mod", "audio/mod");
					_dict.TryAdd(".moov", "video/quicktime");
					_dict.TryAdd(".mov", "video/quicktime");
					_dict.TryAdd(".movie", "video/x-sgi-movie");
					_dict.TryAdd(".mp2", "video/mpeg");
					_dict.TryAdd(".mp3", "audio/mpeg3");
					_dict.TryAdd(".mp4", "video/mp4");
					_dict.TryAdd(".mpa", "audio/mpeg");
					_dict.TryAdd(".mpeg", "video/mpeg");
					_dict.TryAdd(".mpg", "video/mpeg");
					_dict.TryAdd(".mpga", "audio/mpeg");
					_dict.TryAdd(".mpp", "application/vnd.ms-project");
					_dict.TryAdd(".mpt", "application/x-project");
					_dict.TryAdd(".mpv", "application/x-project");
					_dict.TryAdd(".mpx", "application/x-project");
					_dict.TryAdd(".mrc", "application/marc");
					_dict.TryAdd(".ms", "application/x-troff-ms");
					_dict.TryAdd(".mv", "video/x-sgi-movie");
					_dict.TryAdd(".my", "audio/make");
					_dict.TryAdd(".o", "application/octet-stream");
					_dict.TryAdd(".oga", "audio/ogg");
					_dict.TryAdd(".ogg", "video/ogg");
					_dict.TryAdd(".ogv", "video/ogg");
					_dict.TryAdd(".otf", "font/otf");
					_dict.TryAdd(".p", "text/x-pascal");
					_dict.TryAdd(".pas", "text/pascal");
					_dict.TryAdd(".pbm", "image/x-portable-bitmap");
					_dict.TryAdd(".pct", "image/x-pict");
					_dict.TryAdd(".pcx", "image/x-pcx");
					_dict.TryAdd(".pdf", "application/pdf");
					_dict.TryAdd(".pgm", "image/x-portable-greymap");
					_dict.TryAdd(".pic", "image/pict");
					_dict.TryAdd(".pict", "image/pict");
					_dict.TryAdd(".pl", "text/x-script.perl");
					_dict.TryAdd(".png", "image/png");
					_dict.TryAdd(".pot", "application/mspowerpoint");
					_dict.TryAdd(".potx", "application/mspowerpoint");
					_dict.TryAdd(".pps", "application/mspowerpoint");
					_dict.TryAdd(".ppt", "application/mspowerpoint");
					_dict.TryAdd(".pptx", "application/mspowerpoint");
					_dict.TryAdd(".ppz", "application/mspowerpoint");
					_dict.TryAdd(".pre", "application/x-freelance");
					_dict.TryAdd(".ps", "application/postscript");
					_dict.TryAdd(".psd", "application/octet-stream");
					_dict.TryAdd(".py", "text/x-script.python");
					_dict.TryAdd(".qif", "image/x-quicktime");
					_dict.TryAdd(".qt", "video/quicktime");
					_dict.TryAdd(".ra", "audio/x-pn-realaudio");
					_dict.TryAdd(".ram", "audio/x-pn-realaudio");
					_dict.TryAdd(".ras", "application/x-cmu-raster");
					_dict.TryAdd(".rgb", "image/x-rgb");
					_dict.TryAdd(".rm", "audio/x-pn-realaudio");
					_dict.TryAdd(".rt", "text/richtext");
					_dict.TryAdd(".rtf", "application/rtf");
					_dict.TryAdd(".rtx", "application/rtf");
					_dict.TryAdd(".s", "text/x-asm");
					_dict.TryAdd(".sea", "application/sea");
					_dict.TryAdd(".sgm", "text/sgml");
					_dict.TryAdd(".sgml", "text/sgml");
					_dict.TryAdd(".sh", "application/x-bsh");
					_dict.TryAdd(".shar", "application/x-bsh");
					_dict.TryAdd(".shtml", "text/html");
					_dict.TryAdd(".sit", "application/x-stuffit");
					_dict.TryAdd(".snd", "audio/basic");
					_dict.TryAdd(".svc", "text/asp");
					_dict.TryAdd(".svf", "image/x-dwg");
					_dict.TryAdd(".swf", "application/x-shockwave-flash");
					_dict.TryAdd(".t", "application/x-troff");
					_dict.TryAdd(".tar", "application/x-compressed");
					_dict.TryAdd(".tgz", "application/x-compressed");
					_dict.TryAdd(".tif", "image/tiff");
					_dict.TryAdd(".tiff", "image/tiff");
					_dict.TryAdd(".ttf", "font/ttf");
					_dict.TryAdd(".txt", "text/plain");
					_dict.TryAdd(".uu", "application/octet-stream");
					_dict.TryAdd(".uue", "text/x-uuencode");
					_dict.TryAdd(".vbhtml", "text/aspx");
					_dict.TryAdd(".vcs", "text/x-vcalendar");
					_dict.TryAdd(".vda", "application/vda");
					_dict.TryAdd(".vrml", "application/x-vrml");
					_dict.TryAdd(".vsd", "application/x-visio");
					_dict.TryAdd(".vst", "application/x-visio");
					_dict.TryAdd(".vsw", "application/x-visio");
					_dict.TryAdd(".wav", "audio/wav");
					_dict.TryAdd(".wmf", "windows/metafile");
					_dict.TryAdd(".woff", "font/woff");
					_dict.TryAdd(".woff2", "font/woff2");
					_dict.TryAdd(".word", "application/msword");
					_dict.TryAdd(".wp", "application/wordperfect");
					_dict.TryAdd(".wri", "application/mswrite");
					_dict.TryAdd(".wsc", "text/scriplet");
					_dict.TryAdd(".xbm", "image/x-xbitmap");
					_dict.TryAdd(".xif", "image/vnd.xiff");
					_dict.TryAdd(".xl", "application/excel");
					_dict.TryAdd(".xla", "application/excel");
					_dict.TryAdd(".xlb", "application/excel");
					_dict.TryAdd(".xlc", "application/excel");
					_dict.TryAdd(".xld", "application/excel");
					_dict.TryAdd(".xlk", "application/excel");
					_dict.TryAdd(".xll", "application/excel");
					_dict.TryAdd(".xlm", "application/excel");
					_dict.TryAdd(".xls", "application/excel");
					_dict.TryAdd(".xlsx", "application/excel");
					_dict.TryAdd(".xlt", "application/excel");
					_dict.TryAdd(".xlv", "application/excel");
					_dict.TryAdd(".xlw", "application/excel");
					_dict.TryAdd(".xm", "audio/xm");
					_dict.TryAdd(".xml", "text/xml");
					_dict.TryAdd(".xpm", "image/xpm");
					_dict.TryAdd(".zip", "application/zip");
					_dict.TryAdd(".zsh", "text/x-script.zsh");
				}

				return _dict.ToDictionary(pair => pair.Key, pair => pair.Value);
			}
		}
	}
}