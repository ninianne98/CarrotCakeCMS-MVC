# CarrotCakeCMS (MVC 5)
Source code for CarrotCakeCMS (MVC), ASP.NET Framework 4.5

[SITE_CT]: http://www.carrotware.com/contact?from=github-mvc
[REPO_SF]: http://sourceforge.net/projects/carrotcakecmsmvc/
[REPO_GH]: https://github.com/ninianne98/CarrotCakeCMS-MVC/

[DOC_PDF]: http://www.carrotware.com/fileassets/CarrotCakeMVCDevNotes.pdf?from=github-mvc
[DOC]: http://www.carrotware.com/carrotcake-download?from=github-mvc "CarrotCakeCMS User Documentation"
[TMPLT]: http://www.carrotware.com/carrotcake-templates?from=github-mvc
[IDE]: https://visualstudio.microsoft.com/
[VWDISO2013]: https://go.microsoft.com/fwlink/?LinkId=532501&type=ISO&clcid=0x409
[CEISO2013]: https://go.microsoft.com/fwlink/?LinkId=532496&type=ISO&clcid=0x409
[CEISO2015]: http://download.microsoft.com/download/b/e/d/bedddfc4-55f4-4748-90a8-ffe38a40e89f/vs2015.3.com_enu.iso
[SQL]: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
[SSMS]: https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms

## Welcome

Welcome to the GitHub project for CarrotCake CMS MVC, an open source C# project. CarrotCake is a [template-based][TMPLT] MVC5 ASP.NET CMS (content management system) built with C#, SQL Server, jQueryUI, and TinyMCE, providing an intuitive WYSIWYG/drag-and-drop edit experience. This content management system supports multi-tenant webroots with shared databases. 

### If you have found this tool useful please [contact us][SITE_CT].

Source code and [documentation][DOC_PDF] is available on [GitHub][REPO_GH] and [SourceForge][REPO_SF]. Documentation and assemblies can be found [here][DOC].

> [!IMPORTANT]
> **Database Usage Notice**
>
> *   **Automatic Schema Updates:** Running the application against an existing database will automatically attempt to update schema artifacts.
> *   **Backup First:** Always ensure you have a full database backup before running a new version of the code.
> *   **No Database Sharing:** Do not share a single database between the Core, MVC 5, and WebForms editions of CarrotCakeCMS.

## Features

Some features include: blogging engine, configurable date-based blog post URLs, content association with categories and tags, assignment/customization of category and tag URL patterns, simple content feedback collection and review, blog post pagination/indexes (with templating support), designation of default listing blog page (required for search and category/tag links), URL date formatting patterns, RSS feed support for posts and pages, import/export of site content, and import from WordPress XML files.

Other features also include date-based release and retirement of content, site time-zone designation, site search, and the ability to rename the administration folder. It also supports the use of layout views to provide re-use when designing content view templates.

---

## CarrotCakeCMS (MVC 5) Developer Quick Start Guide

Copyright (c) 2011, 2015, 2023, 2026 Samantha Copeland
Licensed under the MIT or GPL v3 License

CarrotCakeCMS (MVC) is maintained by Samantha Copeland

### Install Development Tools

1. **[Visual Studio Community/Express/Pro/Enterprise][IDE]** ([ISO VWD 2013][VWDISO2013], [ISO CE 2013][CEISO2013], or [ISO CE 2015][CEISO2015]) Professional (or higher) editions OK. Typically being developed on VS 2022 Enterprise or VS 2019 Express. Legacy versions (VS 2012–2017) are supported, though the database project may not load in older editions; this is expected and does not impact the core build.
1. **[SQL Server Express 2008 (or higher/later)][SQL]** - Currently vetted on 2008, 2012R2, and 2016 Express.
1. **[SQL Server Management Studio (SSMS)][SSMS]** - Required for managing the database.

### Get the Source Code

1. Go to the repository ([GitHub][REPO_GH] or [SourceForge][REPO_SF]) in a browser

1. Download either a GIT or ZIP archive or connect using either a GIT or SVN client

### Open the Project

1. Start **Visual Studio**

1. Open the **CarrotCakeMVC.sln** solution in the root of the repository

	Note: If your file extensions are hidden, you will not see the ".sln"
	Other SLN files are demo widgets for how to wire in custom code/extensions

1. Edit **Web.config** under the **CMSAdmin** root directory (this corresponds to the **CMSAdminMVC** project)

	- In the `connectionStrings` section, configure `CarrotwareCMSConnectionString` to point to your SQL Server and database name.
		*Note: The user account requires `db_owner` permissions as the application will automatically create (or update) necessary database artifacts.*
	- In `mailSettings`, configure `pickupDirectoryLocation` to a valid directory on your development machine for local email testing.

1. Right-click on **CMSAdminMVC** and select **Set as StartUp Project**

1. Right-click on **CMSAdminMVC** and select **Rebuild**. The project should download all required NuGet packages and compile successfully

	*Note: You may see some compilation warnings; these can generally be ignored.*

1. SQL Server should be running with an empty database matching the one specified in the connection string. If you are running the code a second or later time, it will auto update if there are schema changes (see dbo note above).  Do not share a database between the Core, MVC 5, and WebForms editions.

1. If the database is empty or has pending database changes, you will be greeted with a maintenance screen; follow the link provided.

1. The first time you start up the website, it will create the required artifacts in the database (tables/views/sprocs etc.)

1. Select the run mode (e.g., IIS Express) and click the **Play** button (or hit F5) to launch CarrotCakeCMS

1. When you run the website with an empty user database, you will be prompted to create the first user

1. Once you have created a user, you can go to the login screen, enter the credentials

1. After successfully logging in, you can create and manage your new website

### Using CarrotCakeCMS (MVC 5)

- **Documentation:** See the **[CarrotCakeCMS Documentation][DOC]** for detailed user guides.
