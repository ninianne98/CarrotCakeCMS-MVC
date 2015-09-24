CREATE TABLE [dbo].[carrot_ContentComment] (
    [ContentCommentID] UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_ContentComment_ContentCommentID] DEFAULT (newid()) NOT NULL,
    [Root_ContentID]   UNIQUEIDENTIFIER NOT NULL,
    [CreateDate]       DATETIME         CONSTRAINT [DF_carrot_ContentComment_CreateDate] DEFAULT (getdate()) NOT NULL,
    [CommenterIP]      NVARCHAR (32)    NOT NULL,
    [CommenterName]    NVARCHAR (256)   NOT NULL,
    [CommenterEmail]   NVARCHAR (256)   NOT NULL,
    [CommenterURL]     NVARCHAR (256)   NOT NULL,
    [PostComment]      NVARCHAR (MAX)   NULL,
    [IsApproved]       BIT              NOT NULL,
    [IsSpam]           BIT              NOT NULL,
    CONSTRAINT [PK_carrot_ContentComment] PRIMARY KEY NONCLUSTERED ([ContentCommentID] ASC),
    CONSTRAINT [FK_carrot_ContentComment_Root_ContentID] FOREIGN KEY ([Root_ContentID]) REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
);

