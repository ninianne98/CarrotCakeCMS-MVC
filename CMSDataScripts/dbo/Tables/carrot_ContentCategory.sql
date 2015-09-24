CREATE TABLE [dbo].[carrot_ContentCategory] (
    [ContentCategoryID] UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_ContentCategory_ContentCategoryID] DEFAULT (newid()) NOT NULL,
    [SiteID]            UNIQUEIDENTIFIER NOT NULL,
    [CategoryText]      NVARCHAR (256)   NOT NULL,
    [CategorySlug]      NVARCHAR (256)   NOT NULL,
    [IsPublic]          BIT              NOT NULL,
    CONSTRAINT [PK_carrot_ContentCategory] PRIMARY KEY NONCLUSTERED ([ContentCategoryID] ASC),
    CONSTRAINT [FK_carrot_ContentCategory_SiteID] FOREIGN KEY ([SiteID]) REFERENCES [dbo].[carrot_Sites] ([SiteID])
);

