/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public static class SiteFilename {

		public static string PageAddEditURL {
			get { return SiteData.AdminFolderPath + "PageAddEdit"; }
		}

		public static string PageIndexURL {
			get { return SiteData.AdminFolderPath + "PageIndex"; }
		}

		public static string PageTemplateUpdateURL {
			get { return SiteData.AdminFolderPath + "PageTemplateUpdate"; }
		}

		public static string BlogPostAddEditURL {
			get { return SiteData.AdminFolderPath + "BlogPostAddEdit"; }
		}

		public static string BlogPostIndexURL {
			get { return SiteData.AdminFolderPath + "BlogPostIndex"; }
		}

		public static string BlogPostTemplateUpdateURL {
			get { return SiteData.AdminFolderPath + "BlogPostTemplateUpdate"; }
		}

		public static string CreateFirstAdminURL {
			get { return SiteData.AdminFolderPath + "CreateFirstAdmin"; }
		}

		public static string DatabaseSetupURL {
			get { return SiteData.AdminFolderPath + "DatabaseSetup"; }
		}

		public static string LoginURL {
			get { return SiteData.AdminFolderPath + "Login"; }
		}

		public static string NotAuthorizedURL {
			get { return SiteData.AdminFolderPath + "NotAuthorized"; }
		}

		public static string ForgotPasswordURL {
			get { return SiteData.AdminFolderPath + "ForgotPassword"; }
		}

		public static string UserProfileURL {
			get { return SiteData.AdminFolderPath + "UserProfile"; }
		}

		public static string ChangePasswordURL {
			get { return SiteData.AdminFolderPath + "ChangePassword"; }
		}

		public static string DashboardURL {
			get { return SiteData.AdminFolderPath; }
		}

		public static string SiteInfoURL {
			get { return SiteData.AdminFolderPath + "SiteInfo"; }
		}

		public static string SiteIndexURL {
			get { return SiteData.AdminFolderPath + "SiteIndex"; }
		}

		public static string SiteMapURL {
			get { return SiteData.AdminFolderPath + "SiteMap"; }
		}

		public static string AboutURL {
			get { return SiteData.AdminFolderPath + "About"; }
		}

		public static string SiteTemplateUpdateURL {
			get { return SiteData.AdminFolderPath + "SiteTemplateUpdate"; }
		}

		public static string ContentEditHistoryURL {
			get { return SiteData.AdminFolderPath + "ContentEditHistory"; }
		}

		public static string ModuleIndexURL {
			get { return SiteData.AdminFolderPath + "ModuleIndex"; }
		}

		public static string PageHistoryURL {
			get { return SiteData.AdminFolderPath + "PageHistory"; }
		}

		public static string PageWidgetsURL {
			get { return SiteData.AdminFolderPath + "PageWidgets"; }
		}

		public static string WidgetTimeURL {
			get { return SiteData.AdminFolderPath + "WidgetTime"; }
		}

		public static string WidgetHistoryURL {
			get { return SiteData.AdminFolderPath + "WidgetHistory"; }
		}

		public static string ContentSnippetAddEditURL {
			get { return SiteData.AdminFolderPath + "ContentSnippetAddEdit"; }
		}

		public static string ContentSnippetIndexURL {
			get { return SiteData.AdminFolderPath + "ContentSnippetIndex"; }
		}

		public static string TagIndexURL {
			get { return SiteData.AdminFolderPath + "TagIndex"; }
		}

		public static string TagAddEditURL {
			get { return SiteData.AdminFolderPath + "TagAddEdit"; }
		}

		public static string CategoryIndexURL {
			get { return SiteData.AdminFolderPath + "CategoryIndex"; }
		}

		public static string CategoryAddEditURL {
			get { return SiteData.AdminFolderPath + "CategoryAddEdit"; }
		}

		public static string CommentIndexURL {
			get { return SiteData.AdminFolderPath + "CommentIndex"; }
		}

		public static string PageCommentIndexURL {
			get { return SiteData.AdminFolderPath + "PageCommentIndex"; }
		}

		public static string BlogPostCommentIndexURL {
			get { return SiteData.AdminFolderPath + "BlogPostCommentIndex"; }
		}

		public static string UserEditURL {
			get { return SiteData.AdminFolderPath + "UserEdit"; }
		}

		public static string UserAddURL {
			get { return SiteData.AdminFolderPath + "UserAdd"; }
		}

		public static string UserIndexURL {
			get { return SiteData.AdminFolderPath + "UserIndex"; }
		}

		public static string RoleAddEditURL {
			get { return SiteData.AdminFolderPath + "RoleAddEdit"; }
		}

		public static string RoleIndexURL {
			get { return SiteData.AdminFolderPath + "RoleIndex"; }
		}

		public static string ContentExportURL {
			get { return SiteData.AdminFolderPath + "ContentExport"; }
		}

		public static string SiteExportURL {
			get { return SiteData.AdminFolderPath + "SiteDataExport"; }
		}

		public static string ContentImportURL {
			get { return SiteData.AdminFolderPath + "ContentImport"; }
		}

		public static string SiteImportURL {
			get { return SiteData.AdminFolderPath + "SiteImport"; }
		}

		public static string SiteImportWP_URL {
			get { return SiteData.AdminFolderPath + "SiteImportWP"; }
		}

		public static string AdminModuleViewPath {
			get { return "~/Views/CmsContent/_AdminModule.cshtml"; }
		}

		public static string EditNotifierViewPath {
			get { return "~/Views/CmsContent/_EditNotifier.cshtml"; }
		}

		public static string AdvancedEditViewPath {
			get { return "~/Views/CmsContent/_AdvancedEdit.cshtml"; }
		}

		public static string AdvancedEditHeadViewPath {
			get { return "~/Views/CmsContent/_AdvancedEditHead.cshtml"; }
		}

		public static string MainSiteSpecialViewHead {
			get { return "~/Views/CmsContent/_SpecialHead.cshtml"; }
		}

		public static string MainSiteSpecialViewFoot {
			get { return "~/Views/CmsContent/_SpecialFoot.cshtml"; }
		}
	}
}