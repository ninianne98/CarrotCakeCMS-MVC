GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO

IF NOT EXISTS( select * from [INFORMATION_SCHEMA].[COLUMNS] 
		where table_name = 'membership_Role' and column_name = 'Name') BEGIN

CREATE TABLE [dbo].[membership_Role](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.membership_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[membership_User](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.membership_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[membership_UserClaim](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.membership_UserClaim] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[membership_UserLogin](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.membership_UserLogin] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[membership_UserRole](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.membership_UserRole] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

END

GO

ALTER TABLE [dbo].[carrot_Widget] 
	ALTER COLUMN  [ControlPath] [nvarchar](1024) not null


GO
ALTER PROCEDURE [dbo].[carrot_BlogMonthlyTallies]
    @SiteID uniqueidentifier,
    @ActiveOnly bit,    
    @TakeTop int = 10

/*

exec [carrot_BlogMonthlyTallies] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0', 0, 10

exec [carrot_BlogMonthlyTallies] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0', 0, 16

exec [carrot_BlogMonthlyTallies] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0', 0, 5

*/

AS BEGIN

SET NOCOUNT ON

	DECLARE @UTCDateTime Datetime
	SET @UTCDateTime = GetUTCDate()
	
    DECLARE @ContentTypeID uniqueidentifier
    SELECT  @ContentTypeID = (select top 1 ct.ContentTypeID from dbo.carrot_ContentType ct where ct.ContentTypeValue = 'BlogEntry')

	DECLARE @tblTallies TABLE(
		RowID int identity(1,1),
		SiteID uniqueidentifier,
		ContentCount int,
		DateMonth date,
		DateSlug nvarchar(64)
	)
	
	insert into @tblTallies(SiteID, ContentCount, DateMonth, DateSlug)
		SELECT SiteID, COUNT(Root_ContentID) AS ContentCount, DateMonth, DateSlug
		FROM   (SELECT Root_ContentID, SiteID, ContentTypeID, 
					CONVERT(datetime, CONVERT(nvarchar(100), GoLiveDateLocal, 112)) AS DateMonth, 
					DATENAME(MONTH, GoLiveDateLocal) + ' ' + CAST(YEAR(GoLiveDateLocal) as nvarchar(100)) AS DateSlug
			FROM (SELECT Root_ContentID, SiteID, ContentTypeID, (GoLiveDateLocal - DAY(GoLiveDateLocal) + 1) as GoLiveDateLocal
				FROM [dbo].[carrot_RootContent]
				WHERE SiteID = @SiteID
					AND (PageActive = 1 OR @ActiveOnly = 0)
					AND (GoLiveDate < @UTCDateTime OR @ActiveOnly = 0)
					AND (RetireDate > @UTCDateTime OR @ActiveOnly = 0)
					AND ContentTypeID = @ContentTypeID ) AS Y) AS Z

		GROUP BY SiteID, DateMonth, DateSlug
		ORDER BY DateMonth DESC

	SELECT * FROM @tblTallies WHERE RowID <= @TakeTop ORDER BY RowID

    RETURN(0)

END

GO

IF  NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[membership_User]') 
		AND name = N'UserNameIndex') BEGIN

CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[membership_Role]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[membership_User]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[membership_UserClaim]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[membership_UserLogin]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_RoleId] ON [dbo].[membership_UserRole]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[membership_UserRole]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


END

GO

IF  NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[FK_dbo.membership_UserClaim_dbo.membership_User_UserId]') )
BEGIN

ALTER TABLE [dbo].[membership_UserClaim]  WITH CHECK ADD  CONSTRAINT [FK_dbo.membership_UserClaim_dbo.membership_User_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[membership_User] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[membership_UserClaim] CHECK CONSTRAINT [FK_dbo.membership_UserClaim_dbo.membership_User_UserId]

ALTER TABLE [dbo].[membership_UserLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.membership_UserLogin_dbo.membership_User_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[membership_User] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[membership_UserLogin] CHECK CONSTRAINT [FK_dbo.membership_UserLogin_dbo.membership_User_UserId]

ALTER TABLE [dbo].[membership_UserRole]  WITH CHECK ADD  CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_Role_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[membership_Role] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[membership_UserRole] CHECK CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_Role_RoleId]

ALTER TABLE [dbo].[membership_UserRole]  WITH CHECK ADD  CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_User_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[membership_User] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[membership_UserRole] CHECK CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_User_UserId]

END

GO


GO

IF EXISTS( select * from [INFORMATION_SCHEMA].[COLUMNS] 
		where table_name = 'aspnet_Membership' and column_name = 'UserId') BEGIN

INSERT INTO [dbo].[membership_User](
		[Id]
		,[Email]
		,[EmailConfirmed]
		,[PasswordHash]
		,[SecurityStamp]
		,[PhoneNumber]
		,[PhoneNumberConfirmed]
		,[TwoFactorEnabled]
		,[LockoutEndDateUtc]
		,[LockoutEnabled]
		,[AccessFailedCount]
		,[UserName])
	SELECT lower(m.[UserId]) as [Id], lower(m.[LoweredEmail]) as [Email], 0 as [EmailConfirmed], 
		m.[Password] as [PasswordHash], newid() as [SecurityStamp], 
		null as [PhoneNumber], 0 as [PhoneNumberConfirmed], 0 as [TwoFactorEnabled],
		null as [LockoutEndDateUtc], 1 as [LockoutEnabled], 0 as [AccessFailedCount], lower(u.[LoweredUserName]) as [UserName]
	 FROM [dbo].[aspnet_Membership] as m
		 JOIN [dbo].[aspnet_Users] as u on m.UserId = u.UserId
	 WHERE u.UserName not in(SELECT [UserName] from [dbo].[membership_User])

INSERT INTO [dbo].[membership_Role] ([Id], [Name])
	SELECT lower([RoleId]), [RoleName]
	FROM [dbo].[aspnet_Roles]
	WHERE [RoleName] not in(SELECT [Name] from [dbo].[membership_Role])

INSERT INTO [dbo].[membership_UserRole] ([UserId], [RoleId])
	SELECT lower([UserId]), lower([RoleId])
	FROM [dbo].[aspnet_UsersInRoles]
	WHERE [UserId] not in(SELECT [UserId] from [dbo].[membership_UserRole])

END

GO

IF NOT EXISTS( select * from [INFORMATION_SCHEMA].[COLUMNS] 
		where table_name = 'carrot_UserData' and column_name = 'UserKey') BEGIN
	ALTER TABLE [dbo].[carrot_UserData] ADD [UserKey] [nvarchar](128)
END

GO

UPDATE dbo.[carrot_UserData]
SET [UserKey] = LOWER(cast([UserId] as nvarchar(256)))
WHERE [UserKey] is null

GO

-- optional drop
update [dbo].[carrot_RootContent]
set [FileName] = REPLACE([FileName], '.aspx', ''),
	[PageSlug] = REPLACE([PageSlug], '.aspx', '')

update [dbo].[carrot_RootContent]
set [FileName] = REPLACE([FileName], '.', '-'),
	[PageSlug] = REPLACE([PageSlug], '.', '-')

update [dbo].[carrot_RootContent]
set [FileName] = REPLACE([FileName], '--', '-'),
	[PageSlug] = REPLACE([PageSlug], '--', '-')

--=====================
-- environment specific changes BEGIN
update [dbo].[carrot_Content]
set [PageText] = REPLACE([PageText], '.aspx', '')
where [IsLatestVersion] = 1
and [PageText] like '%.aspx%'

update [dbo].[carrot_Content]
set [PageText] = REPLACE([PageText], '/carrotwarethumb.axd', '/carrotwarethumb.ashx')
where [IsLatestVersion] = 1
and [PageText] like '%/carrotwarethumb.axd%'

update [dbo].[carrot_Content]
set [PageText] = REPLACE([PageText], '/c3-admin/tiny_mce/', '/Assets/tiny_mce/')
where [IsLatestVersion] = 1
and [PageText] like '%/c3-admin/tiny_mce/%'

--=====================
-- environment specific changes END


--=================== starter views BEGIN
-- alter these statements if you have already written some new razor template views
-- plain
update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/cmscontent/plainpage/plainpageview.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%plaintemplate%aspx'


-- imagination
update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/imagination/index.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%cmsTemplates%Imagination%'
	and [TemplateFile] like '%home.aspx'
	and [TemplateFile] not like '%cshtml'

update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/imagination/no-sidebar.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%cmsTemplates%Imagination%'
	and [TemplateFile] like '%no-sidebar.aspx'
	and [TemplateFile] not like '%cshtml'

update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/imagination/right-sidebar.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%cmsTemplates%Imagination%'
	and [TemplateFile] like '%right-sidebar.aspx'
	and [TemplateFile] not like '%cshtml'

update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/imagination/left-sidebar.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%cmsTemplates%Imagination%'
	and [TemplateFile] like '%left-sidebar.aspx'
	and [TemplateFile] not like '%cshtml'

update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/imagination/blog-index.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%cmsTemplates%Imagination%'
	and [TemplateFile] like '%listing.aspx'
	and [TemplateFile] not like '%cshtml'

update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/imagination/blog.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%cmsTemplates%Imagination%'
	and [TemplateFile] like '%post.aspx'
	and [TemplateFile] not like '%cshtml'


-- catch all / citrus island
update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/citrus-island/page.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] not like '%cshtml'

update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/citrus-island/list.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%citrus-island%page.cshtml'
	and Root_ContentID in (select Blog_Root_ContentID from dbo.carrot_Sites where Blog_Root_ContentID is not null)

update [dbo].[carrot_Content]
set [TemplateFile] = '~/views/templates/citrus-island/post.cshtml'
where [IsLatestVersion] = 1
	and [TemplateFile] like '%citrus-island%page.cshtml'
	and Root_ContentID in ( 
				select rc.Root_ContentID
				from carrot_ContentType AS ct 
				inner join carrot_RootContent AS rc on ct.ContentTypeID = rc.ContentTypeID
				where ct.ContentTypeValue like '%blog%' )

--=================== starter views END

--=====================================================================

GO

IF EXISTS( select * from [INFORMATION_SCHEMA].[COLUMNS] 
		where table_name = 'aspnet_Users' and column_name = 'UserId') BEGIN

INSERT INTO dbo.[carrot_UserData](UserId,UserKey)
	select [UserId] , lower([UserId]) as [Id]
	from [dbo].[aspnet_Users]
	where [UserId] not in (select UserId from [carrot_UserData])

END

IF  NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[carrot_UserData_UserKey]') )
BEGIN

ALTER TABLE [dbo].[carrot_UserData]  WITH CHECK ADD  CONSTRAINT [carrot_UserData_UserKey] FOREIGN KEY([UserKey])
REFERENCES [dbo].[membership_User] ([Id])
ALTER TABLE [dbo].[carrot_UserData] CHECK CONSTRAINT [carrot_UserData_UserKey]

END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[FK_carrot_UserData_UserId]') )
BEGIN

ALTER TABLE [dbo].[carrot_Content] DROP CONSTRAINT [carrot_Content_CreditUserId_FK]
ALTER TABLE [dbo].[carrot_Content] DROP CONSTRAINT [carrot_Content_EditUserId_FK]
ALTER TABLE [dbo].[carrot_RootContent] DROP CONSTRAINT [carrot_RootContent_CreateUserId_FK]
ALTER TABLE [dbo].[carrot_UserData] DROP CONSTRAINT [FK_carrot_UserData_UserId]
ALTER TABLE [dbo].[carrot_UserSiteMapping] DROP CONSTRAINT [aspnet_Users_carrot_UserSiteMapping_FK]

END

GO

--carrot_Content	CreditUserId	aspnet_Users	UserId	carrot_Content_CreditUserId_FK	ALTER TABLE [dbo].[carrot_Content] DROP CONSTRAINT [carrot_Content_CreditUserId_FK]
--carrot_Content	EditUserId	aspnet_Users	UserId	carrot_Content_EditUserId_FK	ALTER TABLE [dbo].[carrot_Content] DROP CONSTRAINT [carrot_Content_EditUserId_FK]
--carrot_RootContent	CreateUserId	aspnet_Users	UserId	carrot_RootContent_CreateUserId_FK	ALTER TABLE [dbo].[carrot_RootContent] DROP CONSTRAINT [carrot_RootContent_CreateUserId_FK]
--carrot_UserData	UserId	aspnet_Users	UserId	FK_carrot_UserData_UserId	ALTER TABLE [dbo].[carrot_UserData] DROP CONSTRAINT [FK_carrot_UserData_UserId]
--carrot_UserSiteMapping	UserId	aspnet_Users	UserId	aspnet_Users_carrot_UserSiteMapping_FK	ALTER TABLE [dbo].[carrot_UserSiteMapping] DROP CONSTRAINT [aspnet_Users_carrot_UserSiteMapping_FK]

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[carrot_RootContent_CreateUserId_FK]') )
BEGIN

ALTER TABLE [dbo].[carrot_UserSiteMapping]  WITH CHECK ADD  CONSTRAINT [aspnet_Users_carrot_UserSiteMapping_FK] FOREIGN KEY([UserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])
ALTER TABLE [dbo].[carrot_UserSiteMapping] CHECK CONSTRAINT [aspnet_Users_carrot_UserSiteMapping_FK]

ALTER TABLE [dbo].[carrot_RootContent]  WITH CHECK ADD  CONSTRAINT [carrot_RootContent_CreateUserId_FK] FOREIGN KEY([CreateUserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])
ALTER TABLE [dbo].[carrot_RootContent] CHECK CONSTRAINT [carrot_RootContent_CreateUserId_FK]

ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_Content_EditUserId_FK] FOREIGN KEY([EditUserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])
ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_Content_EditUserId_FK]

ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_Content_CreditUserId_FK] FOREIGN KEY([CreditUserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])
ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_Content_CreditUserId_FK]

END

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_aspnet_UsersInRoles]'))
BEGIN

DROP VIEW [dbo].[vw_aspnet_UsersInRoles]
DROP VIEW [dbo].[vw_aspnet_Users]
DROP VIEW [dbo].[vw_aspnet_Roles]
DROP VIEW [dbo].[vw_aspnet_MembershipUsers]
DROP VIEW [dbo].[vw_aspnet_Applications]

END

GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_Membership_GetAllUsers]') )
BEGIN

DROP PROCEDURE  [dbo].[aspnet_AnyDataInTables]
DROP PROCEDURE  [dbo].[aspnet_Applications_CreateApplication]
DROP PROCEDURE  [dbo].[aspnet_CheckSchemaVersion]
DROP PROCEDURE  [dbo].[aspnet_Membership_ChangePasswordQuestionAndAnswer]
DROP PROCEDURE  [dbo].[aspnet_Membership_CreateUser]
DROP PROCEDURE  [dbo].[aspnet_Membership_FindUsersByEmail]
DROP PROCEDURE  [dbo].[aspnet_Membership_FindUsersByName]
DROP PROCEDURE  [dbo].[aspnet_Membership_GetAllUsers]
DROP PROCEDURE  [dbo].[aspnet_Membership_GetNumberOfUsersOnline]
DROP PROCEDURE  [dbo].[aspnet_Membership_GetPassword]
DROP PROCEDURE  [dbo].[aspnet_Membership_GetPasswordWithFormat]
DROP PROCEDURE  [dbo].[aspnet_Membership_GetUserByEmail]
DROP PROCEDURE  [dbo].[aspnet_Membership_GetUserByName]
DROP PROCEDURE  [dbo].[aspnet_Membership_GetUserByUserId]
DROP PROCEDURE  [dbo].[aspnet_Membership_ResetPassword]
DROP PROCEDURE  [dbo].[aspnet_Membership_SetPassword]
DROP PROCEDURE  [dbo].[aspnet_Membership_UnlockUser]
DROP PROCEDURE  [dbo].[aspnet_Membership_UpdateUser]
DROP PROCEDURE  [dbo].[aspnet_Membership_UpdateUserInfo]
DROP PROCEDURE  [dbo].[aspnet_RegisterSchemaVersion]
DROP PROCEDURE  [dbo].[aspnet_Roles_CreateRole]
DROP PROCEDURE  [dbo].[aspnet_Roles_DeleteRole]
DROP PROCEDURE  [dbo].[aspnet_Roles_GetAllRoles]
DROP PROCEDURE  [dbo].[aspnet_Roles_RoleExists]
DROP PROCEDURE  [dbo].[aspnet_Setup_RemoveAllRoleMembers]
DROP PROCEDURE  [dbo].[aspnet_Setup_RestorePermissions]
DROP PROCEDURE  [dbo].[aspnet_UnRegisterSchemaVersion]
DROP PROCEDURE  [dbo].[aspnet_Users_CreateUser]
DROP PROCEDURE  [dbo].[aspnet_Users_DeleteUser]
DROP PROCEDURE  [dbo].[aspnet_UsersInRoles_AddUsersToRoles]
DROP PROCEDURE  [dbo].[aspnet_UsersInRoles_FindUsersInRole]
DROP PROCEDURE  [dbo].[aspnet_UsersInRoles_GetRolesForUser]
DROP PROCEDURE  [dbo].[aspnet_UsersInRoles_GetUsersInRoles]
DROP PROCEDURE  [dbo].[aspnet_UsersInRoles_IsUserInRole]
DROP PROCEDURE  [dbo].[aspnet_UsersInRoles_RemoveUsersFromRoles]
END

GO


IF EXISTS( select * from [INFORMATION_SCHEMA].[COLUMNS] 
		where table_name = 'aspnet_Users' ) BEGIN

ALTER TABLE [dbo].[aspnet_UsersInRoles] DROP CONSTRAINT [FK_aspnet_UsersInRoles_UserId]
ALTER TABLE [dbo].[aspnet_UsersInRoles] DROP CONSTRAINT [FK_aspnet_UsersInRoles_RoleId]
ALTER TABLE [dbo].[aspnet_Users] DROP CONSTRAINT [FK_aspnet_Users_ApplicationId]
ALTER TABLE [dbo].[aspnet_Roles] DROP CONSTRAINT [FK_aspnet_Roles_ApplicationId]
ALTER TABLE [dbo].[aspnet_Membership] DROP CONSTRAINT [FK_aspnet_Membership_UserId]
ALTER TABLE [dbo].[aspnet_Membership] DROP CONSTRAINT [FK_aspnet_Membership_ApplicationId]

DROP TABLE [dbo].[aspnet_UsersInRoles]
DROP TABLE [dbo].[aspnet_Users]
DROP TABLE [dbo].[aspnet_SchemaVersions]
DROP TABLE [dbo].[aspnet_Roles]
DROP TABLE [dbo].[aspnet_Membership]
DROP TABLE [dbo].[aspnet_Applications]

END

GO

IF EXISTS (SELECT * FROM sys.schemas WHERE name like 'aspnet%' )
BEGIN

DROP SCHEMA [aspnet_Roles_ReportingAccess]
DROP SCHEMA [aspnet_Roles_FullAccess]
DROP SCHEMA [aspnet_Roles_BasicAccess]
DROP SCHEMA [aspnet_Membership_ReportingAccess]
DROP SCHEMA [aspnet_Membership_FullAccess]
DROP SCHEMA [aspnet_Membership_BasicAccess]

END

GO

--======================

GO

IF  EXISTS(select * from sys.database_principals where name LIKE 'aspnet%' and is_fixed_role = 0)
BEGIN

DROP ROLE [aspnet_Membership_BasicAccess]
DROP ROLE [aspnet_Membership_FullAccess]
DROP ROLE [aspnet_Membership_ReportingAccess]
DROP ROLE [aspnet_Roles_BasicAccess]
DROP ROLE [aspnet_Roles_FullAccess]
DROP ROLE [aspnet_Roles_ReportingAccess]

END

GO

--======================

GO
ALTER VIEW [dbo].[vw_carrot_Comment]
AS 

SELECT cc.ContentCommentID, cc.CreateDate, cc.CommenterIP, cc.CommenterName, cc.CommenterEmail, cc.CommenterURL, cc.PostComment, cc.IsApproved, cc.IsSpam, 
	c.Root_ContentID, c.SiteID, c.[FileName], c.PageHead, c.TitleBar, c.NavMenuText, c.IsRetired, c.IsUnReleased, c.RetireDate, c.GoLiveDate, 
	c.PageActive, c.ShowInSiteNav, c.ShowInSiteMap, c.BlockIndex, c.ContentTypeID, c.ContentTypeValue
FROM  dbo.carrot_ContentComment AS cc 
	INNER JOIN dbo.vw_carrot_Content AS c ON cc.Root_ContentID = c.Root_ContentID
WHERE (c.IsLatestVersion = 1)

GO

GO
ALTER VIEW [dbo].[vw_carrot_UserData]
AS 

SELECT mu.Id, mu.Email, mu.EmailConfirmed, mu.PasswordHash, mu.SecurityStamp, mu.PhoneNumber, mu.PhoneNumberConfirmed, 
		mu.TwoFactorEnabled, mu.LockoutEndDateUtc, mu.LockoutEnabled, mu.AccessFailedCount, mu.UserName, ud.UserId, ud.UserKey, 
		ud.UserNickName, ud.FirstName, ud.LastName, ud.UserBio
FROM dbo.membership_User AS mu 
LEFT JOIN carrot_UserData AS ud ON ud.UserKey = mu.Id

GO


--SELECT ud.UserId, ud.UserNickName, ud.FirstName, ud.LastName, ud.UserBio, ud.UserKey
--FROM carrot_UserData AS ud
--INNER JOINmembership_User ON ud.UserId = membership_User.Id


GO
ALTER VIEW [dbo].[vw_carrot_EditorURL]
AS 
-- select top 10 * from [vw_carrot_EditorURL]

select  d.SiteID, d.UserId, d.UserName, d.LoweredEmail, cc2.EditDate, 
		ISNULL(cc2.TheCount, 0) as UseCount, ISNULL(cc3.TheCount, 0) as PublicUseCount,
		'/'+d.Blog_FolderPath +'/'+ d.Blog_EditorPath +'/'+ d.UserName as UserUrl
from (
		select s.SiteID, s.Blog_FolderPath, s.Blog_EditorPath, m.UserId, m.UserName, lower(m.Email) as LoweredEmail
			from [dbo].vw_carrot_UserData m, [dbo].carrot_Sites s
		) as d
	left join (
			select v_cc.EditUserId, v_cc.SiteID, MAX(v_cc.EditDate) as EditDate, COUNT(ContentID) as TheCount
			from dbo.vw_carrot_Content v_cc
			where v_cc.IsLatestVersion = 1
			group by v_cc.EditUserId, v_cc.SiteID
			union
			select v_cc.CreditUserId, v_cc.SiteID, MAX(v_cc.EditDate) as EditDate, COUNT(ContentID) as TheCount
			from dbo.vw_carrot_Content v_cc
			where v_cc.IsLatestVersion = 1
				and v_cc.CreditUserId is not null
			group by v_cc.CreditUserId, v_cc.SiteID		
		
			) as cc2 on d.UserId = cc2.EditUserId
					and d.SiteID = cc2.SiteID
	left join (
			select v_cc.EditUserId, v_cc.SiteID, MAX(v_cc.EditDate) as EditDate, COUNT(ContentID) as TheCount
			from dbo.vw_carrot_Content v_cc
			where v_cc.IsLatestVersion = 1
				and v_cc.PageActive = 1 and v_cc.RetireDate >= GETUTCDATE() and v_cc.GoLiveDate <= GETUTCDATE() 
			group by v_cc.EditUserId, v_cc.SiteID
			union
			select v_cc.CreditUserId, v_cc.SiteID, MAX(v_cc.EditDate) as EditDate, COUNT(ContentID) as TheCount
			from dbo.vw_carrot_Content v_cc
			where v_cc.IsLatestVersion = 1 and v_cc.CreditUserId is not null
				and v_cc.PageActive = 1 and v_cc.RetireDate >= GETUTCDATE() and v_cc.GoLiveDate <= GETUTCDATE() 
			group by v_cc.CreditUserId, v_cc.SiteID	
			) as cc3 on d.UserId = cc3.EditUserId
					and d.SiteID = cc3.SiteID


GO
ALTER VIEW [dbo].[vw_carrot_TagURL]
AS 
-- select top 10 * from [vw_carrot_TagURL]

select  s.SiteID, cc.ContentTagID, cc.TagText, cc.IsPublic, cc2.EditDate, 
		ISNULL(cc2.TheCount, 0) as UseCount, ISNULL(cc3.TheCount, 0) as PublicUseCount,
		'/' + s.Blog_FolderPath + '/' + s.Blog_TagPath + '/' + cc.TagSlug as TagUrl
from [dbo].carrot_Sites as s 
	inner join [dbo].carrot_ContentTag as cc on s.SiteID = cc.SiteID
	left join (select m.ContentTagID, MAX(v_cc.EditDate) as EditDate, COUNT(m.Root_ContentID) as TheCount
				 from [dbo].vw_carrot_Content v_cc
					join [dbo].carrot_TagContentMapping m on v_cc.Root_ContentID = m.Root_ContentID
				 where v_cc.IsLatestVersion = 1
				 group by m.ContentTagID) as cc2 on cc.ContentTagID = cc2.ContentTagID

	left join (select m.ContentTagID, COUNT(m.Root_ContentID) as TheCount
				 from [dbo].vw_carrot_Content v_cc
					join [dbo].carrot_TagContentMapping m on v_cc.Root_ContentID = m.Root_ContentID
				 where v_cc.IsLatestVersion = 1
						and v_cc.PageActive = 1 and v_cc.RetireDate >= GETUTCDATE() and v_cc.GoLiveDate <= GETUTCDATE() 
				 group by m.ContentTagID) as cc3 on cc.ContentTagID = cc3.ContentTagID

GO
ALTER VIEW [dbo].[vw_carrot_CategoryURL]
AS 
-- select top 10 * from [vw_carrot_CategoryURL]

select  s.SiteID, cc.ContentCategoryID, cc.CategoryText, cc.IsPublic, cc2.EditDate, 
		ISNULL(cc2.TheCount, 0) as UseCount, ISNULL(cc3.TheCount, 0) as PublicUseCount,
		'/' + s.Blog_FolderPath + '/' + s.Blog_CategoryPath + '/' + cc.CategorySlug as CategoryUrl
from [dbo].carrot_Sites as s 
	inner join [dbo].carrot_ContentCategory as cc on s.SiteID = cc.SiteID
	left join (select m.ContentCategoryID, MAX(v_cc.EditDate) as EditDate, COUNT(m.Root_ContentID) as TheCount
				 from [dbo].vw_carrot_Content v_cc
					join [dbo].carrot_CategoryContentMapping m on v_cc.Root_ContentID = m.Root_ContentID
				 where v_cc.IsLatestVersion = 1
				 group by m.ContentCategoryID) as cc2 on cc.ContentCategoryID = cc2.ContentCategoryID

	left join (select m.ContentCategoryID, COUNT(m.Root_ContentID) as TheCount
				 from [dbo].vw_carrot_Content v_cc
					join [dbo].carrot_CategoryContentMapping m on v_cc.Root_ContentID = m.Root_ContentID
				 where v_cc.IsLatestVersion = 1
						and v_cc.PageActive = 1 and v_cc.RetireDate >= GETUTCDATE() and v_cc.GoLiveDate <= GETUTCDATE() 
				 group by m.ContentCategoryID) as cc3 on cc.ContentCategoryID = cc3.ContentCategoryID

GO
ALTER VIEW [dbo].[vw_carrot_EditHistory]
AS 

SELECT  rc.SiteID, c.ContentID, c.Root_ContentID, c.IsLatestVersion, c.TitleBar, c.NavMenuText, c.PageHead, c.CreditUserId, 
	c.EditDate, rc.CreateDate, rc.[FileName], ct.ContentTypeID, ct.ContentTypeValue, rc.PageActive, rc.GoLiveDate, rc.RetireDate, 
	c.EditUserId, m.UserName as EditUserName, m.Email as EditEmail, 
	rc.CreateUserId, m2.UserName as CreateUserName, m2.Email as CreateEmail
FROM [dbo].carrot_RootContent AS rc
	INNER JOIN [dbo].carrot_Content AS c ON rc.Root_ContentID = c.Root_ContentID 
	INNER JOIN [dbo].carrot_ContentType AS ct ON rc.ContentTypeID = ct.ContentTypeID
	INNER JOIN [dbo].carrot_UserData AS u ON c.EditUserId = u.UserId 
	INNER JOIN [dbo].membership_User AS m ON u.UserKey = m.Id
	INNER JOIN [dbo].carrot_UserData AS u2 ON rc.CreateUserId = u2.UserId 
	INNER JOIN [dbo].membership_User AS m2 ON u2.UserKey = m2.Id

GO

UPDATE [dbo].[carrot_DataInfo]
set [DataValue] = '20150829' -- match date in the create script
WHERE [DataKey] = 'DBSchema'


GO
