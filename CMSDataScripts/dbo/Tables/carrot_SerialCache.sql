CREATE TABLE [dbo].[carrot_SerialCache] (
    [SerialCacheID]  UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_SerialCache_SerialCacheID] DEFAULT (newid()) NOT NULL,
    [SiteID]         UNIQUEIDENTIFIER NOT NULL,
    [ItemID]         UNIQUEIDENTIFIER NOT NULL,
    [EditUserId]     UNIQUEIDENTIFIER NOT NULL,
    [KeyType]        NVARCHAR (256)   NULL,
    [SerializedData] NVARCHAR (MAX)   NULL,
    [EditDate]       DATETIME         CONSTRAINT [DF_carrot_SerialCache_EditDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [carrot_SerialCache_PK] PRIMARY KEY CLUSTERED ([SerialCacheID] ASC)
);

