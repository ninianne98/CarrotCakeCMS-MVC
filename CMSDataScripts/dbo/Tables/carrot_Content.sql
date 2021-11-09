CREATE TABLE [dbo].[carrot_Content] (
    [ContentID]        UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_Content_ContentID] DEFAULT (newid()) NOT NULL,
    [Root_ContentID]   UNIQUEIDENTIFIER NOT NULL,
    [Parent_ContentID] UNIQUEIDENTIFIER NULL,
    [IsLatestVersion]  BIT              NOT NULL,
    [TitleBar]         NVARCHAR (256)   NULL,
    [NavMenuText]      NVARCHAR (256)   NULL,
    [PageHead]         NVARCHAR (256)   NULL,
    [PageText]         NVARCHAR (MAX)   NULL,
    [LeftPageText]     NVARCHAR (MAX)   NULL,
    [RightPageText]    NVARCHAR (MAX)   NULL,
    [NavOrder]         INT              NOT NULL,
    [EditUserId]       UNIQUEIDENTIFIER NULL,
    [EditDate]         DATETIME         CONSTRAINT [DF_carrot_Content_EditDate] DEFAULT (getdate()) NOT NULL,
    [TemplateFile]     NVARCHAR (256)   NULL,
    [MetaKeyword]      NVARCHAR (1024)  NULL,
    [MetaDescription]  NVARCHAR (1024)  NULL,
    [CreditUserId]     UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_carrot_Content] PRIMARY KEY CLUSTERED ([ContentID] ASC),
    CONSTRAINT [carrot_Content_CreditUserId_FK] FOREIGN KEY ([CreditUserId]) REFERENCES [dbo].[carrot_UserData] ([UserId]),
    CONSTRAINT [carrot_Content_EditUserId_FK] FOREIGN KEY ([EditUserId]) REFERENCES [dbo].[carrot_UserData] ([UserId]),
    CONSTRAINT [carrot_RootContent_carrot_Content_FK] FOREIGN KEY ([Root_ContentID]) REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
);


GO
CREATE NONCLUSTERED INDEX [IDX_carrot_Content_EditUserId]
    ON [dbo].[carrot_Content]([EditUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_carrot_Content_Root_ContentID]
    ON [dbo].[carrot_Content]([Root_ContentID] ASC);

