CREATE TABLE [dbo].[carrot_UserSiteMapping] (
    [UserSiteMappingID] UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_UserSiteMapping_UserSiteMappingID] DEFAULT (newid()) NOT NULL,
    [UserId]            UNIQUEIDENTIFIER NOT NULL,
    [SiteID]            UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [carrot_UserSiteMapping_PK] PRIMARY KEY CLUSTERED ([UserSiteMappingID] ASC),
    CONSTRAINT [aspnet_Users_carrot_UserSiteMapping_FK] FOREIGN KEY ([UserId]) REFERENCES [dbo].[carrot_UserData] ([UserId]),
    CONSTRAINT [carrot_Sites_carrot_UserSiteMapping_FK] FOREIGN KEY ([SiteID]) REFERENCES [dbo].[carrot_Sites] ([SiteID])
);

