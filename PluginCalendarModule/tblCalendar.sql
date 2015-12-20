/****** Object:  Table [dbo].[tblCalendar]    Script Date: 07/16/2012 18:38:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tblCalendar](
	[CalendarID] [uniqueidentifier] NOT NULL,
	[EventDate] [datetime] NULL,
	[EventTitle] [varchar](255) NULL,
	[EventDetail] [varchar](max) NULL,
	[IsActive] [bit] NULL,
	[SiteID] [uniqueidentifier] NULL,
 CONSTRAINT [tblCalendar_PK] PRIMARY KEY CLUSTERED 
(
	[CalendarID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO

/****** Object:  Default [DF_tblCalendar_CalendarID]    Script Date: 07/16/2012 18:38:25 ******/
ALTER TABLE [dbo].[tblCalendar] ADD  CONSTRAINT [DF_tblCalendar_CalendarID]  DEFAULT (newid()) FOR [CalendarID]
GO
