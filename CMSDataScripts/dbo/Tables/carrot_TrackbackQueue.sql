CREATE TABLE [dbo].[carrot_TrackbackQueue] (
    [TrackbackQueueID]  UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_TrackbackQueue_TrackbackQueueID] DEFAULT (newid()) NOT NULL,
    [Root_ContentID]    UNIQUEIDENTIFIER NOT NULL,
    [TrackBackURL]      NVARCHAR (256)   NOT NULL,
    [TrackBackResponse] NVARCHAR (2048)  NULL,
    [ModifiedDate]      DATETIME         NOT NULL,
    [CreateDate]        DATETIME         CONSTRAINT [DF_carrot_TrackbackQueue_CreateDate] DEFAULT (getdate()) NOT NULL,
    [TrackedBack]       BIT              NOT NULL,
    CONSTRAINT [PK_carrot_TrackbackQueue] PRIMARY KEY NONCLUSTERED ([TrackbackQueueID] ASC),
    CONSTRAINT [FK_carrot_TrackbackQueue_Root_ContentID] FOREIGN KEY ([Root_ContentID]) REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
);

