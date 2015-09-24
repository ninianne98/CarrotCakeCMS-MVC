CREATE TABLE [dbo].[carrot_Sites] (
    [SiteID]              UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_Sites_SiteID] DEFAULT (newid()) NOT NULL,
    [MetaKeyword]         NVARCHAR (1024)  NULL,
    [MetaDescription]     NVARCHAR (1024)  NULL,
    [SiteName]            NVARCHAR (256)   NULL,
    [MainURL]             NVARCHAR (128)   NULL,
    [BlockIndex]          BIT              NOT NULL,
    [SiteTagline]         NVARCHAR (1024)  NULL,
    [SiteTitlebarPattern] NVARCHAR (1024)  NULL,
    [Blog_Root_ContentID] UNIQUEIDENTIFIER NULL,
    [Blog_FolderPath]     NVARCHAR (64)    NULL,
    [Blog_CategoryPath]   NVARCHAR (64)    NULL,
    [Blog_TagPath]        NVARCHAR (64)    NULL,
    [Blog_DatePath]       NVARCHAR (64)    NULL,
    [Blog_DatePattern]    NVARCHAR (32)    NULL,
    [TimeZone]            NVARCHAR (128)   NULL,
    [SendTrackbacks]      BIT              NOT NULL,
    [AcceptTrackbacks]    BIT              NOT NULL,
    [Blog_EditorPath]     NVARCHAR (64)    NULL,
    CONSTRAINT [carrot_Sites_PK] PRIMARY KEY CLUSTERED ([SiteID] ASC)
);

