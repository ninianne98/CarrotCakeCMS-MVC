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

	public static class SiteFilename {
		public static string RssFeedUri { get { return "/rss.ashx"; } }
		public static string SiteMapUri { get { return "/sitemap.ashx"; } }

		//===============================

		public static string AdminModuleViewPath { get { return "~/Views/CmsContent/_AdminModule.cshtml"; } }
		public static string AdvancedEditHeadViewPath { get { return "~/Views/CmsContent/_AdvancedEditHead.cshtml"; } }
		public static string AdvancedEditViewPath { get { return "~/Views/CmsContent/_AdvancedEdit.cshtml"; } }
		public static string EditNotifierViewPath { get { return "~/Views/CmsContent/_EditNotifier.cshtml"; } }
		public static string MainSiteSpecialViewFoot { get { return "~/Views/CmsContent/_SpecialFoot.cshtml"; } }
		public static string MainSiteSpecialViewHead { get { return "~/Views/CmsContent/_SpecialHead.cshtml"; } }

		//===============================

		public static string AboutURL { get { return SiteData.AdminFolderPath + SiteActions.About; } }
		public static string BlogPostAddEditURL { get { return SiteData.AdminFolderPath + SiteActions.BlogPostAddEdit; } }
		public static string BlogPostCommentIndexURL { get { return SiteData.AdminFolderPath + SiteActions.BlogPostCommentIndex; } }
		public static string BlogPostIndexURL { get { return SiteData.AdminFolderPath + SiteActions.BlogPostIndex; } }
		public static string BlogPostTemplateUpdateURL { get { return SiteData.AdminFolderPath + SiteActions.BlogPostTemplateUpdate; } }
		public static string CategoryAddEditURL { get { return SiteData.AdminFolderPath + SiteActions.CategoryAddEdit; } }
		public static string CategoryIndexURL { get { return SiteData.AdminFolderPath + SiteActions.CategoryIndex; } }
		public static string ChangePasswordURL { get { return SiteData.AdminFolderPath + SiteActions.ChangePassword; } }
		public static string CommentIndexURL { get { return SiteData.AdminFolderPath + SiteActions.CommentIndex; } }
		public static string ContentEditHistoryURL { get { return SiteData.AdminFolderPath + SiteActions.ContentEditHistory; } }
		public static string ContentEditURL { get { return SiteData.AdminFolderPath + SiteActions.ContentEdit; } }
		public static string ContentExportURL { get { return SiteData.AdminFolderPath + SiteActions.ContentExport; } }
		public static string ContentImportURL { get { return SiteData.AdminFolderPath + SiteActions.ContentImport; } }
		public static string ContentSnippetAddEditURL { get { return SiteData.AdminFolderPath + SiteActions.ContentSnippetAddEdit; } }
		public static string ContentSnippetHistoryURL { get { return SiteData.AdminFolderPath + SiteActions.ContentSnippetHistory; } }
		public static string ContentSnippetIndexURL { get { return SiteData.AdminFolderPath + SiteActions.ContentSnippetIndex; } }
		public static string CreateFirstAdminURL { get { return SiteData.AdminFolderPath + SiteActions.CreateFirstAdmin; } }
		public static string DashboardURL { get { return SiteData.AdminFolderPath; } }
		public static string DatabaseSetupURL { get { return SiteData.AdminFolderPath + SiteActions.DatabaseSetup; } }
		public static string ForgotPasswordURL { get { return SiteData.AdminFolderPath + SiteActions.ForgotPassword; } }
		public static string LoginURL { get { return SiteData.AdminFolderPath + SiteActions.Login; } }
		public static string ModuleIndexURL { get { return SiteData.AdminFolderPath + SiteActions.ModuleIndex; } }
		public static string NotAuthorizedURL { get { return SiteData.AdminFolderPath + SiteActions.NotAuthorized; } }
		public static string PageAddEditURL { get { return SiteData.AdminFolderPath + SiteActions.PageAddEdit; } }
		public static string PageChildSortURL { get { return SiteData.AdminFolderPath + SiteActions.PageChildSort; } }
		public static string PageCommentIndexURL { get { return SiteData.AdminFolderPath + SiteActions.PageCommentIndex; } }
		public static string PageHistoryURL { get { return SiteData.AdminFolderPath + SiteActions.PageHistory; } }
		public static string PageIndexURL { get { return SiteData.AdminFolderPath + SiteActions.PageIndex; } }
		public static string PageTemplateUpdateURL { get { return SiteData.AdminFolderPath + SiteActions.PageTemplateUpdate; } }
		public static string PageWidgetsURL { get { return SiteData.AdminFolderPath + SiteActions.PageWidgets; } }
		public static string RoleAddEditURL { get { return SiteData.AdminFolderPath + SiteActions.RoleAddEdit; } }
		public static string RoleIndexURL { get { return SiteData.AdminFolderPath + SiteActions.RoleIndex; } }
		public static string SiteContentStatusChangeURL { get { return SiteData.AdminFolderPath + SiteActions.SiteContentStatusChange; } }
		public static string SiteExportURL { get { return SiteData.AdminFolderPath + SiteActions.SiteExport; } }
		public static string SiteImportURL { get { return SiteData.AdminFolderPath + SiteActions.SiteImport; } }
		public static string SiteImportWP_URL { get { return SiteData.AdminFolderPath + SiteActions.SiteImportWP; } }
		public static string SiteIndexURL { get { return SiteData.AdminFolderPath + SiteActions.SiteIndex; } }
		public static string SiteInfoURL { get { return SiteData.AdminFolderPath + SiteActions.SiteInfo; } }
		public static string SiteMapURL { get { return SiteData.AdminFolderPath + SiteActions.SiteMap; } }
		public static string SiteSkinEditURL { get { return SiteData.AdminFolderPath + SiteActions.SiteSkinEdit; } }
		public static string SiteSkinIndexURL { get { return SiteData.AdminFolderPath + SiteActions.SiteSkinIndex; } }
		public static string SiteTemplateUpdateURL { get { return SiteData.AdminFolderPath + SiteActions.SiteTemplateUpdate; } }
		public static string TagAddEditURL { get { return SiteData.AdminFolderPath + SiteActions.TagAddEdit; } }
		public static string TagIndexURL { get { return SiteData.AdminFolderPath + SiteActions.TagIndex; } }
		public static string TextWidgetIndexURL { get { return SiteData.AdminFolderPath + SiteActions.TextWidgetIndex; } }
		public static string UserAddURL { get { return SiteData.AdminFolderPath + SiteActions.UserAdd; } }
		public static string UserEditURL { get { return SiteData.AdminFolderPath + SiteActions.UserEdit; } }
		public static string UserIndexURL { get { return SiteData.AdminFolderPath + SiteActions.UserIndex; } }
		public static string UserProfileURL { get { return SiteData.AdminFolderPath + SiteActions.UserProfile; } }
		public static string WidgetHistoryURL { get { return SiteData.AdminFolderPath + SiteActions.WidgetHistory; } }
		public static string WidgetTimeURL { get { return SiteData.AdminFolderPath + SiteActions.WidgetTime; } }
	}
}