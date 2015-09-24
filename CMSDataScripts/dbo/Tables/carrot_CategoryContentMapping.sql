CREATE TABLE [dbo].[carrot_CategoryContentMapping] (
    [CategoryContentMappingID] UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_CategoryContentMapping_CategoryContentMappingID] DEFAULT (newid()) NOT NULL,
    [ContentCategoryID]        UNIQUEIDENTIFIER NOT NULL,
    [Root_ContentID]           UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_carrot_CategoryContentMapping] PRIMARY KEY NONCLUSTERED ([CategoryContentMappingID] ASC),
    CONSTRAINT [FK_carrot_CategoryContentMapping_ContentCategoryID] FOREIGN KEY ([ContentCategoryID]) REFERENCES [dbo].[carrot_ContentCategory] ([ContentCategoryID]),
    CONSTRAINT [FK_carrot_CategoryContentMapping_Root_ContentID] FOREIGN KEY ([Root_ContentID]) REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
);

