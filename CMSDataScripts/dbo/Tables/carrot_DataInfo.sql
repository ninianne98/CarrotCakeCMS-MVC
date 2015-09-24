CREATE TABLE [dbo].[carrot_DataInfo] (
    [DataInfoID] UNIQUEIDENTIFIER CONSTRAINT [DF_carrot_DataInfo_DataInfoID] DEFAULT (newid()) NOT NULL,
    [DataKey]    NVARCHAR (256)   NOT NULL,
    [DataValue]  NVARCHAR (256)   NOT NULL,
    CONSTRAINT [PK_carrot_DataInfo] PRIMARY KEY NONCLUSTERED ([DataInfoID] ASC)
);

