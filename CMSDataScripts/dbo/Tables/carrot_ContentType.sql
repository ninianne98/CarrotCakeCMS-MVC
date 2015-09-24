CREATE TABLE [dbo].[carrot_ContentType] (
    [ContentTypeID]    UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_ContentType_ContentTypeID] DEFAULT (newid()) NOT NULL,
    [ContentTypeValue] NVARCHAR (256)   NOT NULL,
    CONSTRAINT [carrot_ContentType_PK] PRIMARY KEY CLUSTERED ([ContentTypeID] ASC)
);

