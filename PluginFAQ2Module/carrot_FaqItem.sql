
IF NOT EXISTS( select * from [INFORMATION_SCHEMA].[COLUMNS] 
		where table_name = 'carrot_FaqCategory' and column_name = 'FaqCategoryID') BEGIN


	CREATE TABLE [dbo].[carrot_FaqCategory](
		[FaqCategoryID] [uniqueidentifier] NOT NULL,
		[FAQTitle] [varchar](255) NULL,
		[SiteID] [uniqueidentifier] NULL,
	 CONSTRAINT [carrot_FaqCategory_PK] PRIMARY KEY CLUSTERED 
	(
		[FaqCategoryID] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]


	CREATE TABLE [dbo].[carrot_FaqItem](
		[FaqItemID] [uniqueidentifier] NOT NULL,
		[FaqCategoryID] [uniqueidentifier] NOT NULL,
		[ItemOrder] [int] NOT NULL,	
		[IsActive] [bit] NOT NULL,
		[Caption] [varchar](128) NULL,	
		[Question] [varchar](max) NULL,
		[Answer] [varchar](max) NULL,
	 CONSTRAINT [carrot_FaqItem_PK] PRIMARY KEY CLUSTERED 
	(
		[FaqItemID] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]


END


GO


IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[carrot_FaqCategory_carrot_FaqItem_FK]') 
				AND parent_object_id = OBJECT_ID(N'[dbo].[carrot_FaqItem]')) BEGIN

	ALTER TABLE [dbo].[carrot_FaqCategory] ADD  CONSTRAINT [DF_carrot_FaqCategory_FaqCategoryID]  DEFAULT (newid()) FOR [FaqCategoryID]

	ALTER TABLE [dbo].[carrot_FaqItem]  WITH CHECK ADD  CONSTRAINT [carrot_FaqCategory_carrot_FaqItem_FK] FOREIGN KEY([FaqCategoryID])
	REFERENCES [dbo].[carrot_FaqCategory] ([FaqCategoryID])

	ALTER TABLE [dbo].[carrot_FaqItem] CHECK CONSTRAINT [carrot_FaqCategory_carrot_FaqItem_FK]

	ALTER TABLE [dbo].[carrot_FaqItem] ADD  CONSTRAINT [DF_carrot_FaqItem_FAQImageID]  DEFAULT (newid()) FOR [FaqItemID]
	
	ALTER TABLE [dbo].[carrot_FaqItem] ADD  CONSTRAINT [DF_carrot_FaqItem_IsActive]  DEFAULT (1) FOR [IsActive]
	
	ALTER TABLE [dbo].[carrot_FaqItem] ADD  CONSTRAINT [DF_carrot_FaqItem_ItemOrder]  DEFAULT (1) FOR [ItemOrder]

END


GO


