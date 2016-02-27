# CarrotCakeCMS-MVC
Source code for CarrotCakeCMS (MVC)

[REPO_SF]: http://sourceforge.net/projects/carrotcakecmsmvc/
[REPO_GH]: https://github.com/ninianne98/CarrotCakeCMS-MVC/
[REPO_CX]: http://carrotcakecmsmvc.codeplex.com/

[DOC_PDF]: http://www.carrotware.com/fileassets/CarrotCakeMVCDevNotes.pdf?from=github-mvc
[DOC]: http://www.carrotware.com/carrotcake-cms?from=github-mvc "CarrotCakeCMS User Documentation"
[TMPLT]: http://www.carrotware.com/carrotcake-templates?from=github-mvc
[IDE2013]: https://www.visualstudio.com/en-us/news/vs2013-community-vs.aspx
[VWDISO2013]: https://go.microsoft.com/fwlink/?LinkId=532501&type=ISO&clcid=0x409
[CEISO2013]: https://go.microsoft.com/fwlink/?LinkId=532496&type=ISO&clcid=0x409
[SQL]: https://www.microsoft.com/en-us/download/details.aspx?id=22973
[WEB]: http://visualstudiogallery.msdn.microsoft.com/56633663-6799-41d7-9df7-0f2a504ca361

Welcome to the GitHub project for CarrotCake CMS MVC, an open source c# project. CarrotCake is a [template-based][TMPLT] MVC5 ASP.Net CMS (content management system) built with C#, SQL server, jQueryUI, and TinyMCE. This content management system supports multi-tenant webroots with shared databases. 

Source code and [documentation][DOC_PDF] is available on [GitHub][REPO_GH], [SourceForge][REPO_SF], and [Microsoft CodePlex][REPO_CX]. Documentation and assemblies can be found [here][DOC].

Some features include: blogging engine, configurable date based blog post URLs, blog post content association with categories and tags, assignment/customization of category and tag URL patterns, simple content feedback collection and review, blog post pagination/indexes (with templating support), designation of default blog page (required to make search, category links, or tag links function), URL date formatting patterns, RSS feed support for posts and pages, import and export of site content, and import of content from WordPress XML export files (tested against versions 2.9, 3.0, 3.4, and 3.5).

Other features also include date based release and retirement of content - allowing you to queue up content to appear or disappear from your site on a pre-arranged schedule, site timezone designation, ability to rename the administration folder, and site search. Supports the use of master pages to provide re-use when designing site templates.

---

## CarrotCakeCMS Developer Quick Start Guide

Copyright (c) 2011, 2015 Samantha Copeland
Licensed under the MIT or GPL v3 License

CarrotCakeCMS is maintained by Samantha Copeland

### Install Development Tools

1. **[Visual Studio Community or Express 2013][IDE2013]** ([ISO VWD 2013][VWDISO2013] and [ISO CE 2013][CEISO2013]) Professional (or higher) editions OK. VS 2012 mostly OK, the database project won't load, but that's OK because it's just there to maintain a schema history, it is not part of any build event
1. **[SQL Server Express 2008 (or higher/later)][SQL]**

### Get the Source Code

1. Go to the [repository][REPO_GH] in a browser

1. Download either a GIT or ZIP archive or connect using either a GIT or SVN client

### Open the Project

1. Start **Visual Studio 2013**

1. Open **CarrotCakeMVC.sln** solution in the root of the repository

	Note: If your file extensions are hidden, you will not see the ".sln"
	Other SLN files are demo widgets for how to wire in custom code/extensions

1. Edit **Web.config** under **CMSAdmin** root directory (this coresponds to the **CMSAdminMVC** project)

	- In the connectionStrings section, configure the CarrotwareCMSConnectionString value to point to your server and the name of your database.
		Note: the credentials require database owner/dbo level as it will create the database artifacts for you.
	- In the mailSettings, configure the pickupDirectoryLocation to a directory on your development machine (for testing purposes).

1. Right-click on **CMSAdminMVC** and select **Set as StartUp Project**

1. Right-click on **CMSAdminMVC** and select **Rebuild**. The project should download all required NuGet packages and compile successfully

	There may be some warnings, you can ignore them

1. The SQL Server database should be running and an empty database matching the one specified in the connection string. If you are running the code a second or later time, it will auto update if there are schema changes.

1. if the database is empty or has pending database changes, you will be greeted with a maintenance screen, follow the link provided.

1. The first time you start up the website, it will create the required artifacts in the database (tables/views/sprocs etc.)

1. Click the **Play** button in the main toolbar to launch CarrotCakeCMS

1. When you run the website with an empty user database, you will be prompted to create the first user

1. Once you have created a user, you can go to the login screen, enter the credentials

1. After successfully logging in, you can create and manage your new website.

### Using CarrotCakeCMS

For additional information on how to use CarrotCakeCMS, please see the **[CarrotCakeCMS Documentation][DOC]**.
