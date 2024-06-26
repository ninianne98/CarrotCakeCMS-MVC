﻿using Carrotware.CMS.Data;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class CMSConfigHelper : IDisposable {
		private CarrotCMSDataContext _db = CarrotCMSDataContext.Create();

		public CMSConfigHelper() {
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
		}

		private enum CMSConfigFileType {
			AdminMod,
			SkinDef,
			PublicCtrl,
			SiteTextWidgets,
			SiteMapping
		}

		#region IDisposable Members

		public void Dispose() {
			if (_db != null) {
				_db.Dispose();
			}
		}

		#endregion IDisposable Members

		private static string keyAdminMenuModules = "cms_AdminMenuModules";

		private static string keyAdminToolboxModules = "cms_AdminToolboxModules";

		private static string keyPrimarySite = "cms_PrimarySite";

		private static string keyDynamicSite = "cms_DynamicSite";

		private static string keyTemplateFiles = "cms_TemplateFiles";

		private static string keyTemplates = "cms_Templates";

		private static string keyTxtWidgets = "cms_TxtWidgets";

		private static string keyDynSite = "cms_DynSite_";

		public static string keyAdminContent = "cmsAdminContent";

		public static string keyAdminWidget = "cmsAdminWidget";

		public void ResetConfigs() {
			HttpContext.Current.Cache.Remove(keyAdminMenuModules);

			HttpContext.Current.Cache.Remove(keyAdminToolboxModules);

			HttpContext.Current.Cache.Remove(keyPrimarySite);

			HttpContext.Current.Cache.Remove(keyDynamicSite);

			HttpContext.Current.Cache.Remove(keyTemplates);

			HttpContext.Current.Cache.Remove(keyTxtWidgets);

			HttpContext.Current.Cache.Remove(keyTemplateFiles);

			string ModuleKey = keyDynSite + DomainName;
			HttpContext.Current.Cache.Remove(ModuleKey);

			try {
				//VirtualDirectory.RegisterRoutes(true);

				if (SiteData.CurrentSiteExists) {
					SiteData.CurrentSite.LoadTextWidgets();
				}
			} catch (Exception ex) { }

			if (SiteData.CurrentTrustLevel == AspNetHostingPermissionLevel.Unrestricted) {
				HttpRuntime.UnloadAppDomain();
			}
		}

		public static string DomainName {
			get {
				var domName = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
				if ((domName.IndexOf(":") > 0) && (domName.EndsWith(":80") || domName.EndsWith(":443"))) {
					domName = domName.Substring(0, domName.IndexOf(":"));
				}

				return domName.ToLowerInvariant();
			}
		}

		public static bool HasAdminModules() {
			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				return cmsHelper.AdminModules.Any();
			}
		}

		public static FileDataHelper GetFileDataHelper() {
			string fileTypes = null;

			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();
			if (config.FileManagerConfig != null && !string.IsNullOrEmpty(config.FileManagerConfig.BlockedExtensions)) {
				fileTypes = config.FileManagerConfig.BlockedExtensions;
			}

			return new FileDataHelper(fileTypes);
		}

		private static DataSet ReadDataSetConfig(CMSConfigFileType cfg, string sPath) {
			string sPlugCfg = "default.config";
			string sRealPath = HttpContext.Current.Server.MapPath(sPath);
			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();

			int iExpectedTblCount = 1;

			switch (cfg) {
				case CMSConfigFileType.SiteTextWidgets:
					sPlugCfg = sRealPath + config.ConfigFileLocation.TextContentProcessors;
					break;

				case CMSConfigFileType.SiteMapping:
					sPlugCfg = sRealPath + config.ConfigFileLocation.SiteMapping;
					break;

				case CMSConfigFileType.AdminMod:
					sPlugCfg = sRealPath + "Admin.config";
					iExpectedTblCount = 2;
					break;

				case CMSConfigFileType.PublicCtrl:
					sPlugCfg = sRealPath + "Public.config";
					break;

				case CMSConfigFileType.SkinDef:
					sPlugCfg = sRealPath + "Skin.config";
					break;

				default:
					sPlugCfg = sRealPath + "default.config";
					iExpectedTblCount = -1;
					break;
			}

			DataSet ds = new DataSet();
			if (File.Exists(sPlugCfg) && iExpectedTblCount > 0) {
				ds.ReadXml(sPlugCfg);
			}

			if (ds == null) {
				ds = new DataSet();
			}

			int iTblCount = ds.Tables.Count;

			// if dataset has wrong # of tables, build out more tables
			if (iTblCount < iExpectedTblCount) {
				for (int t = iTblCount; t <= iExpectedTblCount; t++) {
					ds.Tables.Add(new DataTable());
					ds.AcceptChanges();
				}
			}

			if (iExpectedTblCount > 0) {
				iTblCount = ds.Tables.Count;

				string table1Name = string.Empty;

				List<string> reqCols0 = new List<string>();
				List<string> reqCols1 = new List<string>();

				switch (cfg) {
					case CMSConfigFileType.AdminMod:
						reqCols0.Add("caption");
						reqCols0.Add("area");
						table1Name = "pluginlist";

						reqCols1.Add("area");
						reqCols1.Add("pluginlabel");
						reqCols1.Add("menuorder");
						reqCols1.Add("action");
						reqCols1.Add("controller");
						reqCols1.Add("usepopup");
						reqCols1.Add("visible");

						break;

					case CMSConfigFileType.PublicCtrl:
						//case CMSConfigFileType.PublicControls:
						reqCols0.Add("filepath");
						reqCols0.Add("crtldesc");
						table1Name = "ctrlfile";

						break;

					case CMSConfigFileType.SkinDef:
						reqCols0.Add("templatefile");
						reqCols0.Add("filedesc");
						table1Name = "pagenames";

						break;

					case CMSConfigFileType.SiteTextWidgets:
						reqCols0.Add("pluginassembly");
						reqCols0.Add("pluginname");
						table1Name = "plugin";

						break;

					case CMSConfigFileType.SiteMapping:
						reqCols0.Add("domname");
						reqCols0.Add("siteid");
						table1Name = "sitedetail";

						break;

					default:
						reqCols0.Add("caption");
						reqCols0.Add("pluginid");
						table1Name = "none";

						break;
				}

				if (ds.Tables.Contains(table1Name)) {
					//validate that the dataset has the right table configuration
					DataTable dt0 = ds.Tables[table1Name];
					foreach (string c in reqCols0) {
						if (!dt0.Columns.Contains(c)) {
							DataColumn dc = new DataColumn(c);
							dc.DataType = System.Type.GetType("System.String"); // add if not found

							dt0.Columns.Add(dc);
							dt0.AcceptChanges();
						}
					}

					for (int iTbl = 1; iTbl < iTblCount; iTbl++) {
						DataTable dt = ds.Tables[iTbl];
						foreach (string c in reqCols1) {
							if (!dt.Columns.Contains(c)) {
								DataColumn dc = new DataColumn(c);
								dc.DataType = System.Type.GetType("System.String"); // add if not found

								dt.Columns.Add(dc);
								dt.AcceptChanges();
							}
						}
					}
				}
			}

			return ds;
		}

		public static DateTime CalcNearestFiveMinTime(DateTime dateIn) {
			dateIn = dateIn.AddMinutes(-2);
			int iMin = 5 * (dateIn.Minute / 5);

			DateTime dateOut = dateIn.AddMinutes(0 - dateIn.Minute).AddMinutes(iMin);

			return dateOut;
		}

		public string GetFolderPrefix(string sDirPath) {
			return FileDataHelper.MakeWebFolderPath(sDirPath);
		}

		public static object GetCacheItem(string key) {
			if (HttpContext.Current.Cache[key] != null) {
				return HttpContext.Current.Cache[key];
			}
			return null;
		}

		public static string GetCacheItemString(string key) {
			var item = GetCacheItem(key);
			return item != null ? item.ToString() : null;
		}

		public List<CMSAdminModule> AdminModules {
			get {
				var modules = new List<CMSAdminModule>();

				bool bCached = false;

				try {
					modules = (List<CMSAdminModule>)GetCacheItem(keyAdminMenuModules);
					if (modules != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					List<CMSAdminModuleMenu> _ctrls = new List<CMSAdminModuleMenu>();
					modules = new List<CMSAdminModule>();

					foreach (var p in modules) {
						p.PluginMenus = (from c in _ctrls
										 where c.AreaKey == p.AreaKey
										 orderby c.Caption, c.SortOrder
										 select c).ToList();
					}

					modules = modules.Union(GetModulesByDirectory()).ToList();

					HttpContext.Current.Cache.Insert(keyAdminMenuModules, modules, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}

				return modules.OrderBy(m => m.PluginName).ToList();
			}
		}

		private List<CMSPlugin> GetPluginsByDirectory() {
			var plugins = new List<CMSPlugin>();

			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();

			string sPlugCfg = HttpContext.Current.Server.MapPath(config.ConfigFileLocation.PluginPath);

			if (Directory.Exists(sPlugCfg)) {
				string[] subdirs;
				try {
					subdirs = Directory.GetDirectories(sPlugCfg);
				} catch {
					subdirs = null;
				}

				if (subdirs != null) {
					foreach (string theDir in subdirs) {
						string sTplDef = theDir + @"\Public.config";

						if (File.Exists(sTplDef)) {
							string sPathPrefix = GetFolderPrefix(theDir);
							DataSet ds = ReadDataSetConfig(CMSConfigFileType.PublicCtrl, sPathPrefix);

							var p2 = (from d in ds.Tables[0].AsEnumerable()
									  select new CMSPlugin {
										  SortOrder = 100,
										  FilePath = d.Field<string>("filepath"),
										  Caption = d.Field<string>("crtldesc")
									  }).Where(x => x.FilePath.Contains(":")).ToList();

							foreach (var p in p2.Where(x => x.FilePath.ToLowerInvariant().EndsWith("html")).Select(x => x)) {
								string[] path = p.FilePath.Split(':');
								if (path.Length > 2 && !string.IsNullOrEmpty(path[2])
											&& (path[2].ToLowerInvariant().EndsWith(".cshtml") || path[2].ToLowerInvariant().EndsWith(".vbhtml"))) {
									path[2] = "~" + sPathPrefix + path[2];
									p.FilePath = string.Join(":", path);
								}
							}

							var p3 = (from d in ds.Tables[0].AsEnumerable()
									  select new CMSPlugin {
										  SortOrder = 100,
										  FilePath = "~" + sPathPrefix + d.Field<string>("filepath"),
										  Caption = d.Field<string>("crtldesc")
									  }).Where(x => !x.FilePath.Contains(":")).ToList();

							plugins = plugins.Union(p2).Union(p3).ToList();
						}
					}
				}
			}

			plugins.Where(x => x.FilePath.StartsWith("~~/")).ToList().ForEach(r => r.FilePath = r.FilePath.Replace("~~/", "~/"));
			plugins.Where(x => x.FilePath.Contains("//")).ToList().ForEach(r => r.FilePath = r.FilePath.Replace("//", "/"));

			return plugins;
		}

		public List<CMSPlugin> GetPluginsInFolder(string sPathPrefix) {
			var plugins = new List<CMSPlugin>();

			if (!sPathPrefix.EndsWith("/")) {
				sPathPrefix = sPathPrefix + "/";
			}

			if (!string.IsNullOrEmpty(sPathPrefix)) {
				DataSet ds = ReadDataSetConfig(CMSConfigFileType.PublicCtrl, sPathPrefix);

				plugins = (from d in ds.Tables[0].AsEnumerable()
						   select new CMSPlugin {
							   SortOrder = 100,
							   FilePath = "~" + sPathPrefix + d.Field<string>("filepath"),
							   Caption = d.Field<string>("crtldesc")
						   }).ToList();
			}

			plugins.Where(x => x.FilePath.StartsWith("~~/")).ToList().ForEach(r => r.FilePath = r.FilePath.Replace("~~/", "~/"));

			return plugins;
		}

		public CMSAdminModuleMenu GetCurrentAdminModuleControl() {
			HttpRequest request = HttpContext.Current.Request;
			string pf = string.Empty;
			CMSAdminModuleMenu cc = null;

			if (request.QueryString["pf"] != null) {
				pf = request.QueryString["pf"].ToString();

				CMSAdminModule mod = (from m in AdminModules
									  where m.AreaKey == PluginAreaPath
									  select m).FirstOrDefault();

				cc = (from m in mod.PluginMenus
					  orderby m.Caption, m.SortOrder
					  where m.Action == pf
					  select m).FirstOrDefault();
			}

			return cc;
		}

		public static string PluginAreaPath {
			get {
				string path = HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"];

				int i1 = path.IndexOf("/") + 1;
				int i2 = path.IndexOf("/", 2) - 1;

				return path.Substring(i1, i2);
			}
		}

		public List<CMSAdminModuleMenu> GetCurrentAdminModuleControlList() {
			HttpRequest request = HttpContext.Current.Request;
			string pf = string.Empty;

			CMSAdminModule mod = (from m in AdminModules
								  where m.AreaKey == PluginAreaPath
								  select m).FirstOrDefault();

			return (from m in mod.PluginMenus
					orderby m.Caption, m.SortOrder
					select m).ToList();
		}

		public void GetFile(string remoteFile, string localFile) {
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			Uri remoteUri = new Uri(remoteFile);
			string serverPath = HttpContext.Current.Server.MapPath(localFile);
			bool bExists = File.Exists(serverPath);

			if (!bExists) {
				using (var webClient = new WebClient()) {
					try {
						webClient.DownloadFile(remoteUri, serverPath);
					} catch (Exception ex) {
						if (ex is WebException) {
							WebException webException = (WebException)ex;
							var resp = (HttpWebResponse)webException.Response;
							if (!(resp.StatusCode == HttpStatusCode.NotFound)) {
								throw;
							}
						} else {
							throw;
						}
					}
				}
			}
		}

		private List<CMSAdminModule> GetModulesByDirectory() {
			var plugins = new List<CMSAdminModule>();

			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();

			string sPlugCfg = HttpContext.Current.Server.MapPath(config.ConfigFileLocation.PluginPath);

			if (Directory.Exists(sPlugCfg)) {
				string[] subdirs;
				try {
					subdirs = Directory.GetDirectories(sPlugCfg);
				} catch {
					subdirs = null;
				}

				if (subdirs != null) {
					foreach (string theDir in subdirs) {
						string sTplDef = theDir + @"\Admin.config";

						if (File.Exists(sTplDef)) {
							string sPathPrefix = GetFolderPrefix(theDir);
							DataSet ds = ReadDataSetConfig(CMSConfigFileType.AdminMod, sPathPrefix);

							var modules = (from d in ds.Tables[0].AsEnumerable()
										   select new CMSAdminModule {
											   PluginName = d.Field<string>("caption"),
											   AreaKey = d.Field<string>("area")
										   }).OrderBy(x => x.PluginName).ToList();

							var ctrls = (from d in ds.Tables[1].AsEnumerable()
										 select new CMSAdminModuleMenu {
											 Caption = d.Field<string>("pluginlabel"),
											 SortOrder = string.IsNullOrEmpty(d.Field<string>("menuorder")) ? -1 : int.Parse(d.Field<string>("menuorder")),
											 Action = d.Field<string>("action"),
											 Controller = d.Field<string>("controller"),
											 UsePopup = string.IsNullOrEmpty(d.Field<string>("usepopup")) ? false : Convert.ToBoolean(d.Field<string>("usepopup")),
											 IsVisible = string.IsNullOrEmpty(d.Field<string>("visible")) ? false : Convert.ToBoolean(d.Field<string>("visible")),
											 AreaKey = d.Field<string>("area")
										 }).OrderBy(x => x.Caption).OrderBy(x => x.SortOrder).ToList();

							foreach (var p in modules) {
								p.PluginMenus = (from c in ctrls
												 where c.AreaKey == p.AreaKey
												 orderby c.Caption, c.SortOrder
												 select c).ToList();
							}

							plugins = plugins.Union(modules).ToList();
						}
					}
				}
			}

			return plugins;
		}

		public List<CMSTextWidgetPicker> GetAllWidgetSettings(Guid siteID) {
			List<TextWidget> lstPreferenced = TextWidget.GetSiteTextWidgets(siteID);

			List<string> lstInUse = lstPreferenced.Select(x => x.TextWidgetAssembly).Distinct().ToList();

			List<string> lstAvail = TextWidgets.Select(x => x.AssemblyString).Distinct().ToList();

			List<CMSTextWidgetPicker> lstExisting = (from p in lstPreferenced
													 join t in TextWidgets on p.TextWidgetAssembly equals t.AssemblyString
													 select new CMSTextWidgetPicker {
														 TextWidgetPickerID = p.TextWidgetID,
														 AssemblyString = p.TextWidgetAssembly,
														 DisplayName = t.DisplayName,
														 ProcessBody = p.ProcessBody,
														 ProcessPlainText = p.ProcessPlainText,
														 ProcessHTMLText = p.ProcessHTMLText,
														 ProcessComment = p.ProcessComment,
														 ProcessSnippet = p.ProcessSnippet,
													 }).ToList();

			List<CMSTextWidgetPicker> lstConfigured1 = (from t in TextWidgets
														where !lstInUse.Contains(t.AssemblyString)
														select new CMSTextWidgetPicker {
															TextWidgetPickerID = Guid.NewGuid(),
															AssemblyString = t.AssemblyString,
															DisplayName = t.DisplayName,
															ProcessBody = false,
															ProcessPlainText = false,
															ProcessHTMLText = false,
															ProcessComment = false,
															ProcessSnippet = false,
														}).ToList();

			lstExisting = lstExisting.Union(lstConfigured1).ToList();

			List<CMSTextWidgetPicker> lstConfigured2 = (from p in lstPreferenced
														where !lstAvail.Contains(p.TextWidgetAssembly)
														select new CMSTextWidgetPicker {
															TextWidgetPickerID = p.TextWidgetID,
															AssemblyString = p.TextWidgetAssembly,
															DisplayName = string.Empty,
															ProcessBody = p.ProcessBody,
															ProcessPlainText = p.ProcessPlainText,
															ProcessHTMLText = p.ProcessHTMLText,
															ProcessComment = p.ProcessComment,
															ProcessSnippet = p.ProcessSnippet,
														}).ToList();

			lstExisting = lstExisting.Union(lstConfigured2).ToList();

			return lstExisting;
		}

		public List<CMSPlugin> ToolboxPlugins {
			get {
				var plugins = new List<CMSPlugin>();

				bool bCached = false;

				try {
					plugins = (List<CMSPlugin>)GetCacheItem(keyAdminToolboxModules);
					if (plugins != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					int iSortOrder = 0;

					List<CMSPlugin> p1 = new List<CMSPlugin>();

					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Generic HTML", FilePath = "CLASS:Carrotware.CMS.UI.Components.ContentRichText, Carrotware.CMS.UI.Components" });
					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Plain Text", FilePath = "CLASS:Carrotware.CMS.UI.Components.ContentPlainText, Carrotware.CMS.UI.Components" });
					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Content Snippet", FilePath = "CLASS:Carrotware.CMS.UI.Components.ContentSnippetText, Carrotware.CMS.UI.Components" });

					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Top Level Navigation", FilePath = "CLASS:Carrotware.CMS.UI.Components.TopLevelNavigation, Carrotware.CMS.UI.Components" });
					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Two Level Navigation", FilePath = "CLASS:Carrotware.CMS.UI.Components.TwoLevelNavigation, Carrotware.CMS.UI.Components" });

					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Child Navigation", FilePath = "CLASS:Carrotware.CMS.UI.Components.ChildNavigation, Carrotware.CMS.UI.Components" });
					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Second Level/ Sibling Navigation", FilePath = "CLASS:Carrotware.CMS.UI.Components.SecondLevelNavigation, Carrotware.CMS.UI.Components" });
					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Most Recent Updated", FilePath = "CLASS:Carrotware.CMS.UI.Components.MostRecentUpdated, Carrotware.CMS.UI.Components" });

					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "Multi Level Nav List", FilePath = "CLASS:Carrotware.CMS.UI.Components.MultiLevelNavigation, Carrotware.CMS.UI.Components" });

					p1.Add(new CMSPlugin { SystemPlugin = true, SortOrder = iSortOrder++, Caption = "IFRAME content wrapper", FilePath = "CLASS:Carrotware.CMS.UI.Components.IFrameWidgetWrapper, Carrotware.CMS.UI.Components" });

					plugins = p1.Union(GetPluginsByDirectory()).ToList();

					plugins.Where(x => x.FilePath.StartsWith("~~/")).ToList().ForEach(r => r.FilePath = r.FilePath.Replace("~~/", "~/"));

					HttpContext.Current.Cache.Insert(keyAdminToolboxModules, plugins, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}

				return plugins.OrderBy(p => p.SystemPlugin).OrderBy(p => p.Caption).OrderBy(p => p.SortOrder).ToList();
			}
		}

		private List<CMSTemplate> GetTemplatesByDirectory() {
			var plugins = new List<CMSTemplate>();

			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();

			string sPlugCfg = HttpContext.Current.Server.MapPath(config.ConfigFileLocation.TemplatePath);

			if (Directory.Exists(sPlugCfg)) {
				string[] subdirs;
				try {
					subdirs = Directory.GetDirectories(sPlugCfg);
				} catch {
					subdirs = null;
				}

				if (subdirs != null) {
					foreach (string theDir in subdirs) {
						string sTplDef = theDir + @"\Skin.config";

						if (File.Exists(sTplDef)) {
							string sPathPrefix = GetFolderPrefix(theDir);
							DataSet ds = ReadDataSetConfig(CMSConfigFileType.SkinDef, sPathPrefix);

							var p2 = (from d in ds.Tables[0].AsEnumerable()
									  select new CMSTemplate {
										  TemplatePath = "~/" + (sPathPrefix + d.Field<string>("templatefile").ToLowerInvariant()).ToLowerInvariant(),
										  EncodedPath = string.Empty,
										  Caption = d.Field<string>("filedesc")
									  }).ToList();

							plugins = plugins.Union(p2).ToList();

							plugins.Where(x => x.TemplatePath.StartsWith("~~/")).ToList()
								.ForEach(r => r.TemplatePath = r.TemplatePath.Replace("~~/", "~/"));
							plugins.Where(x => x.TemplatePath.Contains("//")).ToList()
								.ForEach(r => r.TemplatePath = r.TemplatePath.Replace("//", "/"));

							plugins.ForEach(r => r.EncodedPath = EncodeBase64(r.TemplatePath));
						}
					}
				}
			}

			return plugins;
		}

		public List<CMSTemplate> Templates {
			get {
				List<CMSTemplate> plugins = null;
				bool bCached = false;

				try {
					plugins = (List<CMSTemplate>)GetCacheItem(keyTemplates);
					if (plugins != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					var site = SiteData.CurrentSite;
					plugins = new List<CMSTemplate>();

					var t1 = new CMSTemplate();
					t1.TemplatePath = site.TemplateFilename;
					t1.EncodedPath = EncodeBase64(site.TemplateFilename);
					t1.Caption = string.Format("    {0} [*]  ", CarrotWeb.DisplayNameFor<SiteData>(x => x.TemplateFilename));
					plugins.Add(t1);

					var t2 = new CMSTemplate();
					t2.TemplatePath = site.TemplateBWFilename;
					t2.EncodedPath = EncodeBase64(site.TemplateBWFilename);
					t2.Caption = string.Format("   {0} [*]  ", CarrotWeb.DisplayNameFor<SiteData>(x => x.TemplateBWFilename));
					plugins.Add(t2);
				}

				if (!bCached) {
					var p2 = GetTemplatesByDirectory();

					plugins = plugins.Union(p2.Where(t => !SiteData.DefaultTemplates.Contains(t.TemplatePath.ToLowerInvariant()))).ToList();

					HttpContext.Current.Cache.Insert(keyTemplates, plugins, null, DateTime.Now.AddMinutes(3), Cache.NoSlidingExpiration);
				}

				return plugins.OrderBy(t => t.Caption).ToList();
			}
		}

		public List<CMSTextWidget> TextWidgets {
			get {
				List<CMSTextWidget> plugins = null;
				bool bCached = false;

				try {
					plugins = (List<CMSTextWidget>)GetCacheItem(keyTxtWidgets);
					if (plugins != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					plugins = new List<CMSTextWidget>();
				}

				if (!bCached) {
					DataSet ds = ReadDataSetConfig(CMSConfigFileType.SiteTextWidgets, "~/");

					plugins = (from d in ds.Tables[0].AsEnumerable()
							   select new CMSTextWidget {
								   AssemblyString = d.Field<string>("pluginassembly"),
								   DisplayName = d.Field<string>("pluginname")
							   }).ToList();

					HttpContext.Current.Cache.Insert(keyTxtWidgets, plugins, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}

				return plugins.OrderBy(t => t.DisplayName).ToList();
			}
		}

		public static List<DynamicSite> SiteList {
			get {
				var sites = new List<DynamicSite>();

				bool bCached = false;

				try {
					sites = (List<DynamicSite>)GetCacheItem(keyDynamicSite);
					if (sites != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					DataSet ds = ReadDataSetConfig(CMSConfigFileType.SiteMapping, "~/");

					sites = (from d in ds.Tables[0].AsEnumerable()
							 select new DynamicSite {
								 DomainName = string.IsNullOrEmpty(d.Field<string>("domname")) ? string.Empty : d.Field<string>("domname").ToLowerInvariant(),
								 SiteID = new Guid(d.Field<string>("siteid"))
							 }).ToList();

					HttpContext.Current.Cache.Insert(keyDynamicSite, sites, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}
				return sites;
			}
		}

		public static Guid PrimarySiteID {
			get {
				Guid site = Guid.Empty;
				bool bCached = false;

				try {
					var val = GetCacheItemString(keyPrimarySite);
					if (val != null) {
						site = new Guid(val);
						bCached = val.Length > 10;
					} else {
						bCached = false;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					site = CarrotCakeConfig.GetConfig().MainConfig.SiteID.Value;

					HttpContext.Current.Cache.Insert(keyPrimarySite, site, null, DateTime.Now.AddMinutes(3), Cache.NoSlidingExpiration);
				}

				return site;
			}
		}

		public static DynamicSite DynSite {
			get {
				DynamicSite site = new DynamicSite();

				string ModuleKey = keyDynSite + DomainName;
				bool bCached = false;

				try {
					site = (DynamicSite)GetCacheItem(ModuleKey);
					if (site != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if ((SiteList.Any()) && !bCached) {
					site = (from ss in SiteList
							where ss.DomainName == DomainName
							select ss).FirstOrDefault();

					HttpContext.Current.Cache.Insert(ModuleKey, site, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}
				return site;
			}
		}

		public static bool CheckRequestedFileExistence(string templateFileName, Guid siteID) {
			var templates = GetTmplateStatus();

			CMSFilePath tmp = templates.Where(x => x.TemplateFile.ToLowerInvariant() == templateFileName.ToLowerInvariant() && x.SiteID == siteID).FirstOrDefault();

			if (tmp == null) {
				tmp = new CMSFilePath(templateFileName, siteID);
				templates.Add(tmp);
#if DEBUG
				Debug.WriteLine(" ================ " + DateTime.UtcNow.ToString() + " ================");
				Debug.WriteLine("Grabbed file : CheckRequestedFileExistence(string templateFileName, Guid siteID) " + templateFileName);
#endif
			}

			SaveTmplateStatus(templates);

			return tmp.FileExists;
		}

		public static bool CheckFileExistence(string templateFileName) {
			var templates = GetTmplateStatus();

			CMSFilePath tmp = templates.Where(x => x.TemplateFile.ToLowerInvariant() == templateFileName.ToLowerInvariant() && x.SiteID == Guid.Empty).FirstOrDefault();

			if (tmp == null) {
				tmp = new CMSFilePath(templateFileName);
				templates.Add(tmp);
#if DEBUG
				Debug.WriteLine(" ================ " + DateTime.UtcNow.ToString() + " ================");
				Debug.WriteLine("Grabbed file : CheckFileExistence(string templateFileName) " + templateFileName);
#endif
			}

			SaveTmplateStatus(templates);

			return tmp.FileExists;
		}

		private static void SaveTmplateStatus(List<CMSFilePath> fileState) {
			HttpContext.Current.Cache.Insert(keyTemplateFiles, fileState, null, DateTime.Now.AddMinutes(10), Cache.NoSlidingExpiration);
		}

		private static List<CMSFilePath> GetTmplateStatus() {
			var templates = new List<CMSFilePath>();

			try { templates = (List<CMSFilePath>)GetCacheItem(keyTemplateFiles); } catch { }

			if (templates == null) {
				templates = new List<CMSFilePath>();
			}

			templates.RemoveAll(x => x.DateChecked < DateTime.UtcNow.AddSeconds(-30));

			templates.RemoveAll(x => x.DateChecked < DateTime.UtcNow.AddSeconds(-10) && x.SiteID != Guid.Empty);

			return templates;
		}

		//=========================

		public static TimeZoneInfo GetLocalTimeZoneInfo() {
			TimeZoneInfo oTZ = TimeZoneInfo.Local;

			return oTZ;
		}

		public static TimeZoneInfo GetSiteTimeZoneInfo(string timeZoneIdentifier) {
			TimeZoneInfo oTZ = GetLocalTimeZoneInfo();

			if (!string.IsNullOrEmpty(timeZoneIdentifier)) {
				try { oTZ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneIdentifier); } catch { }
			}

			return oTZ;
		}

		public static DateTime ConvertUTCToSiteTime(DateTime dateUTC, string timeZoneIdentifier) {
			TimeZoneInfo oTZ = GetSiteTimeZoneInfo(timeZoneIdentifier);

			return TimeZoneInfo.ConvertTimeFromUtc(dateUTC, oTZ);
		}

		public static DateTime ConvertSiteTimeToUTC(DateTime dateSite, string timeZoneIdentifier) {
			TimeZoneInfo oTZ = GetSiteTimeZoneInfo(timeZoneIdentifier);

			return TimeZoneInfo.ConvertTimeToUtc(dateSite, oTZ);
		}

		//===================

		public static string InactivePagePrefix {
			get {
				return "&#9746; ";
			}
		}

		public static string RetiredPagePrefix {
			get {
				return "&#9851; ";
			}
		}

		public static string UnreleasedPagePrefix {
			get {
				return "&#9888; ";
			}
		}

		public static string PendingDeletePrefix {
			get {
				return "&#9940; ";
			}
		}

		public static List<SiteNav> TweakData(List<SiteNav> navs) {
			if (navs != null) {
				navs.RemoveAll(x => x.ShowInSiteNav == false && x.ContentType == ContentPageType.PageType.ContentEntry);

				navs.ForEach(q => FixNavLinkText(q));
			}

			return navs;
		}

		public static string EncodeNavText(string text) {
			return HttpUtility.HtmlEncode(text);
		}

		public static SiteNav FixNavLinkText(SiteNav nav) {
			if (nav != null && !nav.MadeSafe) {
				nav.MadeSafe = true;
				nav.NavMenuText = EncodeNavText(nav.NavMenuText);
				nav.PageHead = EncodeNavText(nav.PageHead);
				nav.TitleBar = EncodeNavText(nav.TitleBar);

				if (!nav.PageActive) {
					nav.NavMenuText = InactivePagePrefix + nav.NavMenuText;
					nav.PageHead = InactivePagePrefix + nav.PageHead;
					nav.TitleBar = InactivePagePrefix + nav.TitleBar;
				}
				if (nav.IsRetired) {
					nav.NavMenuText = RetiredPagePrefix + nav.NavMenuText;
					nav.PageHead = RetiredPagePrefix + nav.PageHead;
					nav.TitleBar = RetiredPagePrefix + nav.TitleBar;
				}
				if (nav.IsUnReleased) {
					nav.NavMenuText = UnreleasedPagePrefix + nav.NavMenuText;
					nav.PageHead = UnreleasedPagePrefix + nav.PageHead;
					nav.TitleBar = UnreleasedPagePrefix + nav.TitleBar;
				}
			}
			return nav;
		}

		public static ContentPage FixNavLinkText(ContentPage cp) {
			if (cp != null && !cp.MadeSafe) {
				cp.MadeSafe = true;
				cp.NavMenuText = EncodeNavText(cp.NavMenuText);
				cp.PageHead = EncodeNavText(cp.PageHead);
				cp.TitleBar = EncodeNavText(cp.TitleBar);

				if (!cp.PageActive) {
					cp.NavMenuText = InactivePagePrefix + cp.NavMenuText;
					cp.PageHead = InactivePagePrefix + cp.PageHead;
					cp.TitleBar = InactivePagePrefix + cp.TitleBar;
				}
				if (cp.IsRetired) {
					cp.NavMenuText = RetiredPagePrefix + cp.NavMenuText;
					cp.PageHead = RetiredPagePrefix + cp.PageHead;
					cp.TitleBar = RetiredPagePrefix + cp.TitleBar;
				}
				if (cp.IsUnReleased) {
					cp.NavMenuText = UnreleasedPagePrefix + cp.NavMenuText;
					cp.PageHead = UnreleasedPagePrefix + cp.PageHead;
					cp.TitleBar = UnreleasedPagePrefix + cp.TitleBar;
				}
			}

			return cp;
		}

		public static PostComment IdentifyLinkAsInactive(PostComment pc) {
			if (pc != null) {
				if (!pc.IsApproved) {
					pc.CommenterName = InactivePagePrefix + pc.CommenterName;
				}
				if (pc.IsSpam) {
					pc.CommenterName = RetiredPagePrefix + pc.CommenterName;
				}
			}

			return pc;
		}

		//=====================

		public static string DecodeBase64(string text) {
			return text.DecodeBase64();
		}

		public static string EncodeBase64(string text) {
			return text.EncodeBase64();
		}

		public void OverrideKey(Guid guidContentID) {
			filePage = null;
			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				filePage = pageHelper.FindContentByID(SiteData.CurrentSiteID, guidContentID);
			}
		}

		public void OverrideKey(string sPageName) {
			filePage = null;
			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				filePage = pageHelper.FindByFilename(SiteData.CurrentSiteID, sPageName);
			}
		}

		protected ContentPage filePage = null;

		protected void LoadGuids() {
			if (filePage == null) {
				using (ContentPageHelper pageHelper = new ContentPageHelper()) {
					if (SiteData.IsPageSampler && filePage == null) {
						filePage = ContentPageHelper.GetSamplerView();
					} else {
						if (SiteData.CurrentScriptName.ToLowerInvariant().StartsWith(SiteData.AdminFolderPath)) {
							Guid guidPage = Guid.Empty;
							if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pageid"])) {
								guidPage = new Guid(HttpContext.Current.Request.QueryString["pageid"].ToString());
							}
							filePage = pageHelper.FindContentByID(SiteData.CurrentSiteID, guidPage);
						} else {
							filePage = pageHelper.FindByFilename(SiteData.CurrentSiteID, SiteData.CurrentScriptName);
						}
					}
				}
			}
		}

		public ContentPage cmsAdminContent {
			get {
				ContentPage c = null;
				try {
					string xml = GetSerialized(keyAdminContent);
					if (!string.IsNullOrEmpty(xml)) {
						var xmlSerializer = new XmlSerializer(typeof(ContentPage));
						object genpref = null;
						using (var stringReader = new StringReader(xml)) {
							genpref = xmlSerializer.Deserialize(stringReader);
						}
						c = genpref as ContentPage;
					}
				} catch (Exception ex) { }
				return c;
			}
			set {
				if (value == null) {
					ClearSerialized(keyAdminContent);
				} else {
					var xmlSerializer = new XmlSerializer(typeof(ContentPage));
					string xml = string.Empty;
					using (var stringWriter = new StringWriter()) {
						xmlSerializer.Serialize(stringWriter, value);
						xml = stringWriter.ToString();
					}
					SaveSerialized(keyAdminContent, xml);
				}
			}
		}

		public List<Widget> cmsAdminWidget {
			get {
				List<Widget> c = null;
				string xml = GetSerialized(keyAdminWidget);
				if (!string.IsNullOrEmpty(xml)) {
					var xmlSerializer = new XmlSerializer(typeof(List<Widget>));
					object genpref = null;
					using (var stringReader = new StringReader(xml)) {
						genpref = xmlSerializer.Deserialize(stringReader);
					}
					c = genpref as List<Widget>;
				}
				return c;
			}
			set {
				if (value == null) {
					ClearSerialized(keyAdminWidget);
				} else {
					var xmlSerializer = new XmlSerializer(typeof(List<Widget>));
					string xml = string.Empty;
					using (var stringWriter = new StringWriter()) {
						xmlSerializer.Serialize(stringWriter, value);
						xml = stringWriter.ToString();
					}
					SaveSerialized(keyAdminWidget, xml);
				}
			}
		}

		public static void SaveSerialized(Guid itemID, string sKey, string sData) {
			using (var db = CarrotCMSDataContext.Create()) {
				bool bAdd = false;

				carrot_SerialCache itm = CompiledQueries.SearchSeriaCache(db, itemID, sKey);

				if (itm == null) {
					bAdd = true;
					itm = new carrot_SerialCache();
					itm.SerialCacheID = Guid.NewGuid();
					itm.SiteID = SiteData.CurrentSiteID;
					itm.ItemID = itemID;
					itm.EditUserId = SecurityData.CurrentUserGuid;
					itm.KeyType = sKey;
				}

				itm.SerializedData = sData;
				itm.EditDate = DateTime.UtcNow;

				if (bAdd) {
					db.carrot_SerialCaches.InsertOnSubmit(itm);
				}
				db.SubmitChanges();
			}
		}

		public static string GetSerialized(Guid itemID, string sKey) {
			string sData = string.Empty;
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_SerialCache itm = CompiledQueries.SearchSeriaCache(db, itemID, sKey);

				if (itm != null) {
					sData = itm.SerializedData;
				}
			}

			return sData;
		}

		public static bool ClearSerialized(Guid itemID, string sKey) {
			bool bRet = false;
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_SerialCache itm = CompiledQueries.SearchSeriaCache(db, itemID, sKey);

				if (itm != null) {
					db.carrot_SerialCaches.DeleteOnSubmit(itm);
					db.SubmitChanges();
					bRet = true;
				}
			}
			return bRet;
		}

		private void SaveSerialized(string sKey, string sData) {
			LoadGuids();
			if (filePage != null) {
				CMSConfigHelper.SaveSerialized(filePage.Root_ContentID, sKey, sData);
			}
		}

		private string GetSerialized(string sKey) {
			string sData = string.Empty;
			LoadGuids();

			if (filePage != null) {
				sData = CMSConfigHelper.GetSerialized(filePage.Root_ContentID, sKey);
			}
			return sData;
		}

		private bool ClearSerialized(string sKey) {
			LoadGuids();
			if (filePage != null) {
				return CMSConfigHelper.ClearSerialized(filePage.Root_ContentID, sKey);
			} else {
				return false;
			}
		}

		public static void CleanUpSerialData() {
			using (var db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_SerialCache> lst = (from c in db.carrot_SerialCaches
													  where c.EditDate < DateTime.UtcNow.AddHours(-6)
													  select c);

				db.carrot_SerialCaches.BatchDelete(lst);
				db.SubmitChanges();
			}
		}
	}
}