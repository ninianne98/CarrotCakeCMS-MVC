CREATE TABLE [dbo].[carrot_RootContent] (
    [Root_ContentID]   UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_RootContent_Root_ContentID] DEFAULT (newid()) NOT NULL,
    [SiteID]           UNIQUEIDENTIFIER NOT NULL,
    [Heartbeat_UserId] UNIQUEIDENTIFIER NULL,
    [EditHeartbeat]    DATETIME         NULL,
    [FileName]         NVARCHAR (256)   NOT NULL,
    [PageActive]       BIT              NOT NULL,
    [CreateDate]       DATETIME         CONSTRAINT [DF_carrot_RootContent_CreateDate] DEFAULT (getdate()) NOT NULL,
    [ContentTypeID]    UNIQUEIDENTIFIER NOT NULL,
    [PageSlug]         NVARCHAR (256)   NULL,
    [PageThumbnail]    NVARCHAR (128)   NULL,
    [GoLiveDate]       DATETIME         CONSTRAINT [DF_carrot_RootContent_GoLiveDate] DEFAULT (getutcdate()) NOT NULL,
    [RetireDate]       DATETIME         CONSTRAINT [DF_carrot_RootContent_RetireDate] DEFAULT (getutcdate()) NOT NULL,
    [GoLiveDateLocal]  DATETIME         CONSTRAINT [DF_carrot_RootContent_GoLiveDateLocal] DEFAULT (getutcdate()) NOT NULL,
    [ShowInSiteNav]    BIT              NOT NULL,
    [CreateUserId]     UNIQUEIDENTIFIER NOT NULL,
    [ShowInSiteMap]    BIT              NOT NULL,
    [BlockIndex]       BIT              NOT NULL,
    CONSTRAINT [carrot_RootContent_PK] PRIMARY KEY CLUSTERED ([Root_ContentID] ASC),
    CONSTRAINT [carrot_ContentType_carrot_RootContent_FK] FOREIGN KEY ([ContentTypeID]) REFERENCES [dbo].[carrot_ContentType] ([ContentTypeID]),
    CONSTRAINT [carrot_RootContent_CreateUserId_FK] FOREIGN KEY ([CreateUserId]) REFERENCES [dbo].[carrot_UserData] ([UserId]),
    CONSTRAINT [carrot_Sites_carrot_RootContent_FK] FOREIGN KEY ([SiteID]) REFERENCES [dbo].[carrot_Sites] ([SiteID])
);


GO
CREATE NONCLUSTERED INDEX [IDX_carrot_RootContent_SiteID]
    ON [dbo].[carrot_RootContent]([SiteID] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_carrot_RootContent_CreateUserId]
    ON [dbo].[carrot_RootContent]([CreateUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_carrot_RootContent_ContentTypeID]
    ON [dbo].[carrot_RootContent]([ContentTypeID] ASC);

