CREATE TABLE [dbo].[carrot_TextWidget] (
    [TextWidgetID]       UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_TextWidget_TextWidgetID] DEFAULT (newid()) NOT NULL,
    [SiteID]             UNIQUEIDENTIFIER NOT NULL,
    [TextWidgetAssembly] NVARCHAR (256)   NOT NULL,
    [ProcessBody]        BIT              NOT NULL,
    [ProcessPlainText]   BIT              NOT NULL,
    [ProcessHTMLText]    BIT              NOT NULL,
    [ProcessComment]     BIT              NOT NULL,
    [ProcessSnippet]     BIT              NOT NULL,
    CONSTRAINT [PK_carrot_TextWidget] PRIMARY KEY NONCLUSTERED ([TextWidgetID] ASC),
    CONSTRAINT [FK_carrot_TextWidget_SiteID] FOREIGN KEY ([SiteID]) REFERENCES [dbo].[carrot_Sites] ([SiteID])
);

