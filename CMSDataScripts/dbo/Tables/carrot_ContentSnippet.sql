CREATE TABLE [dbo].[carrot_ContentSnippet] (
    [ContentSnippetID]      UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_ContentSnippet_ContentSnippetID] DEFAULT (newid()) NOT NULL,
    [Root_ContentSnippetID] UNIQUEIDENTIFIER NOT NULL,
    [IsLatestVersion]       BIT              NOT NULL,
    [EditUserId]            UNIQUEIDENTIFIER NOT NULL,
    [EditDate]              DATETIME         NOT NULL,
    [ContentBody]           NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_carrot_ContentSnippet] PRIMARY KEY CLUSTERED ([ContentSnippetID] ASC),
    CONSTRAINT [FK_carrot_ContentSnippet_Root_ContentSnippetID] FOREIGN KEY ([Root_ContentSnippetID]) REFERENCES [dbo].[carrot_RootContentSnippet] ([Root_ContentSnippetID])
);

