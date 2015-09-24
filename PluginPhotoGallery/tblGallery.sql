

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblGallery]') AND type in (N'U')) BEGIN

	CREATE TABLE [dbo].[tblGallery](
		[GalleryID] [uniqueidentifier] NOT NULL,
		[GalleryTitle] [varchar](255) NULL,
		[SiteID] [uniqueidentifier] NULL,
	 CONSTRAINT [tblGallery_PK] PRIMARY KEY CLUSTERED 
	(
		[GalleryID] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

END


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblGalleryImage]') AND type in (N'U')) BEGIN

	CREATE TABLE [dbo].[tblGalleryImage](
		[GalleryImageID] [uniqueidentifier] NOT NULL,
		[GalleryImage] [varchar](512) NULL,
		[ImageOrder]  int NULL,
		[GalleryID] [uniqueidentifier] NULL,
	 CONSTRAINT [tblGalleryImage_PK] PRIMARY KEY CLUSTERED 
	(
		[GalleryImageID] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

END


GO


IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_tblGallery_GalleryID]') AND type = 'D') BEGIN

	ALTER TABLE [dbo].[tblGallery] ADD  CONSTRAINT [DF_tblGallery_GalleryID]  DEFAULT (newid()) FOR [GalleryID]

END


IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[tblGallery_tblGalleryImage_FK]') AND parent_object_id = OBJECT_ID(N'[dbo].[tblGalleryImage]')) BEGIN

	ALTER TABLE [dbo].[tblGalleryImage]  WITH CHECK ADD  CONSTRAINT [tblGallery_tblGalleryImage_FK] FOREIGN KEY([GalleryID])
	REFERENCES [dbo].[tblGallery] ([GalleryID])
	ALTER TABLE [dbo].[tblGalleryImage] CHECK CONSTRAINT [tblGallery_tblGalleryImage_FK]

END


IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_tblGalleryImage_GalleryImageID]') AND type = 'D') BEGIN

	ALTER TABLE [dbo].[tblGalleryImage] ADD  CONSTRAINT [DF_tblGalleryImage_GalleryImageID]  DEFAULT (newid()) FOR [GalleryImageID]

END


GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblGalleryImageMeta]') AND type in (N'U')) BEGIN

	CREATE TABLE [dbo].[tblGalleryImageMeta](
		[GalleryImageMetaID] [uniqueidentifier] NOT NULL,
		[GalleryImage] [varchar](512) NULL,
		[ImageTitle] [varchar](256) NULL,
		[ImageMetaData] [varchar](max) NULL,
		[SiteID] [uniqueidentifier] NULL,
	 CONSTRAINT [tblGalleryImageMeta_PK] PRIMARY KEY CLUSTERED 
	(
		[GalleryImageMetaID] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

END


GO


IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_tblGalleryImageMeta_GalleryImageMetaID]') AND type = 'D') BEGIN

	ALTER TABLE [dbo].[tblGalleryImageMeta] ADD  CONSTRAINT [DF_tblGalleryImageMeta_GalleryImageMetaID]  DEFAULT (newid()) FOR [GalleryImageMetaID]

END


GO
