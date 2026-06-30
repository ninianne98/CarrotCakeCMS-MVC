using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web.Hosting;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	public class CmsStringVirtualFile : VirtualFile {
		private string _viewContent = string.Empty;
		private string _path = string.Empty;

		public CmsStringVirtualFile(string path, string viewContent) : base(path) {
			_path = path;
			_viewContent = viewContent;

			_path = VirtualViewFileProvider.ScrubFilename(_path);
		}

		public CmsStringVirtualFile(VirtualView viewdata) : base(viewdata?.Path) {
			_path = viewdata.Path;
			_viewContent = viewdata.Markup;

			_path = VirtualViewFileProvider.ScrubFilename(_path);
		}

		public override Stream Open() {
			// Convert the string into a byte stream for the Razor engine
			var bytes = Encoding.UTF8.GetBytes(_viewContent);
			return new MemoryStream(bytes);
		}
	}

	//====================

	public class VirtualView {

		public VirtualView() { }

		public VirtualView(string path, string markup) {
			this.Path = path;
			this.Markup = markup;
			this.LoadDate = DateTime.Now.AddMinutes(-3);
		}

		public string Path { get; set; } = string.Empty;
		public string Markup { get; set; } = string.Empty;
		public DateTime LoadDate { get; set; } = DateTime.MinValue;
	}

	//====================

	public class VirtualViewFileProvider : VirtualPathProvider {
		internal static string VirtualPath = "~/CmsInMemory/Views/";

		private static readonly ConcurrentDictionary<string, VirtualView> _virtualFiles
							= new ConcurrentDictionary<string, VirtualView>(StringComparer.OrdinalIgnoreCase);

		public static class PartialViews {
			public static string Contact { get { return BuildVirtualPath("_form_contact"); } }
			public static string Login { get { return BuildVirtualPath("_form_loginout"); } }
			public static string PagedComments { get { return BuildVirtualPath("_paged_comments"); } }
			public static string PagedData { get { return BuildVirtualPath("_paged_data"); } }
		}

		internal static string ScrubFilename(string path) {
			if (string.IsNullOrWhiteSpace(path)) { return path; }

			if (PathProbably(path) == false) { return path; }

			var newPath = path.StartsWith("~") ? path : string.Format("~{0}", path);

			if (newPath.StartsWith(VirtualPath, StringComparison.OrdinalIgnoreCase) == false) { return newPath; }

			if (newPath.ToLowerInvariant().EndsWith("html") == false) {
				newPath = newPath + ".cshtml";
			}

			return newPath;
		}

		internal static bool PathProbably(string path) {
			if (string.IsNullOrWhiteSpace(path)) { return false; }

			if (path.StartsWith(VirtualPath, StringComparison.OrdinalIgnoreCase)
						|| string.Format("~{0}", path).StartsWith(VirtualPath, StringComparison.OrdinalIgnoreCase)) {
				return true;
			}

			return false;
		}

		public static string BuildVirtualPath(string viewName) {
			if (viewName.EndsWith("html") == false) {
				viewName = viewName + ".cshtml";
			}

			viewName = Path.GetFileName(viewName);

			return VirtualPath + viewName;
		}

		public static void BulkRegister() {
			var validViews = new string[] { PartialViews.Contact, PartialViews.Login, PartialViews.PagedComments, PartialViews.PagedData };

			foreach (var view in validViews) {
				var tmp = RegisterView(view);
			}
		}

		public static string RegisterView(string scriptKey) {
			if (scriptKey.EndsWith("html") == false) {
				scriptKey = scriptKey + ".cshtml";
			}

			scriptKey = Path.GetFileName(scriptKey);
			var path = VirtualPath + scriptKey;
			var key = path.ToLowerInvariant().ToLowerInvariant();

			if (_virtualFiles.ContainsKey(key) == false) {
				var viewText = CarrotCakeHtml.GetEmbededView(scriptKey);
				var namespaces = CarrotCakeHtml.GetRazorNamespaces();

				// the namespaces are needed to be directly in the view
				// as when rendering embeded the web.config is out of scope
				// append the values in the web.config and build the nsUsings

				var nsUsings = string.Join(Environment.NewLine, namespaces.Select(x => "@using " + x));

				viewText = nsUsings + Environment.NewLine + Environment.NewLine + viewText;

				var viewdata = new VirtualView(key, viewText);

				_virtualFiles[key] = viewdata;
			}

			return path;
		}

		private bool IsInMemoryPath(string path) {
			if (PathProbably(path) == false) { return false; }

			path = ScrubFilename(path);

			return path.StartsWith(VirtualPath, StringComparison.OrdinalIgnoreCase);
		}

		public override CacheDependency GetCacheDependency(string path, IEnumerable pathDependencies, DateTime utcStart) {
			if (IsInMemoryPath(path)) {
				return null;
			}

			return base.GetCacheDependency(path, pathDependencies, utcStart);
		}

		public override bool FileExists(string path) {
			path = ScrubFilename(path);

			var key = path.ToLowerInvariant().ToLowerInvariant();

			if (IsInMemoryPath(path)) {
				return _virtualFiles.ContainsKey(key);
			}
			return Previous.FileExists(key);
		}

		public override VirtualFile GetFile(string path) {
			path = ScrubFilename(path);

			if (IsInMemoryPath(path) && _virtualFiles.TryGetValue(path, out var content)) {
				return new CmsStringVirtualFile(content);
			}

			return Previous.GetFile(path);
		}
	}
}