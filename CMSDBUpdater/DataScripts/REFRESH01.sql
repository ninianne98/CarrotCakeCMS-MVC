SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER VIEW [dbo].[vw_carrot_Content]
AS 

SELECT rc.Root_ContentID, rc.SiteID, rc.Heartbeat_UserId, rc.EditHeartbeat, rc.[FileName], rc.PageActive, rc.ShowInSiteNav, rc.ShowInSiteMap, rc.BlockIndex,
		rc.CreateUserId, rc.CreateDate, c.ContentID, c.Parent_ContentID, c.IsLatestVersion, c.TitleBar, c.NavMenuText, c.PageHead, 
		c.PageText, c.LeftPageText, c.RightPageText, c.NavOrder, c.EditUserId, c.CreditUserId, c.EditDate, c.TemplateFile, c.MetaKeyword, c.MetaDescription,
		cvh.VersionCount, ct.ContentTypeID, ct.ContentTypeValue, rc.PageSlug, rc.PageThumbnail, s.TimeZone,
		rc.RetireDate, rc.GoLiveDate, rc.GoLiveDateLocal,
		cast(case when rc.RetireDate <= GetUTCDate() then 1 else 0 end as bit) as IsRetired,
		cast(case when rc.GoLiveDate >= GetUTCDate() then 1 else 0 end as bit) as IsUnReleased
FROM [dbo].carrot_RootContent AS rc 
	INNER JOIN [dbo].carrot_Sites AS s ON rc.SiteID = s.SiteID 
	INNER JOIN [dbo].carrot_Content AS c ON rc.Root_ContentID = c.Root_ContentID 
	INNER JOIN [dbo].carrot_ContentType AS ct ON rc.ContentTypeID = ct.ContentTypeID
	INNER JOIN (SELECT COUNT(*) VersionCount, Root_ContentID 
				FROM [dbo].carrot_Content
				GROUP BY Root_ContentID 
				) cvh on rc.Root_ContentID = cvh.Root_ContentID


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER VIEW [dbo].[vw_carrot_UserData]
AS 

SELECT mu.Id, mu.Email, mu.EmailConfirmed, mu.PasswordHash, mu.SecurityStamp, mu.PhoneNumber, mu.PhoneNumberConfirmed, 
		mu.TwoFactorEnabled, mu.LockoutEndDateUtc, mu.LockoutEnabled, mu.AccessFailedCount, mu.UserName, ud.UserId, ud.UserKey, 
		ud.UserNickName, ud.FirstName, ud.LastName, ud.UserBio
FROM dbo.membership_User AS mu 
LEFT JOIN carrot_UserData AS ud ON ud.UserKey = mu.Id


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER VIEW [dbo].[vw_carrot_ContentChild]
AS 

SELECT DISTINCT cc.SiteID, cc.Root_ContentID, cc.[FileName], 
          cc.RetireDate, cc.GoLiveDate, 
          cc.IsRetired, cc.IsUnReleased, 
          cp.Root_ContentID as Parent_ContentID, cp.[FileName] AS ParentFileName,
          cp.RetireDate AS ParentRetireDate, cp.GoLiveDate AS ParentGoLiveDate, 
          cp.IsRetired as IsParentRetired, cp.IsUnReleased as IsParentUnReleased
FROM dbo.vw_carrot_Content AS cc 
	INNER JOIN dbo.vw_carrot_Content AS cp ON cc.Parent_ContentID = cp.Root_ContentID
WHERE cp.IsLatestVersion = 1 AND cc.IsLatestVersion = 1
	AND cc.SiteID = cp.SiteID


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER VIEW [dbo].[vw_carrot_CategoryCounted]
AS 

SELECT cc.ContentCategoryID, cc.SiteID, cc.CategoryText, cc.CategorySlug, cc.IsPublic, ISNULL(cc2.TheCount, 0) AS UseCount
FROM dbo.carrot_ContentCategory AS cc 
LEFT JOIN
      (SELECT ContentCategoryID, COUNT(Root_ContentID) AS TheCount
        FROM dbo.carrot_CategoryContentMapping
        GROUP BY ContentCategoryID) AS cc2 ON cc.ContentCategoryID = cc2.ContentCategoryID


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER VIEW [dbo].[vw_carrot_ContentSnippet]
AS 

SELECT csr.Root_ContentSnippetID, csr.SiteID, csr.ContentSnippetName, csr.ContentSnippetSlug, csr.CreateUserId, csr.CreateDate, 
	csr.ContentSnippetActive, cs.ContentSnippetID, cs.IsLatestVersion, cs.EditUserId, cs.EditDate, cs.ContentBody, 
	csr.Heartbeat_UserId, csr.EditHeartbeat, csr.GoLiveDate, csr.RetireDate,
	cast(case when csr.RetireDate < GetUTCDate() then 1 else 0 end as bit) as IsRetired,
	cast(case when csr.GoLiveDate > GetUTCDate() then 1 else 0 end as bit) as IsUnReleased,
	csvh.VersionCount
FROM carrot_RootContentSnippet AS csr 
	INNER JOIN carrot_ContentSnippet AS cs ON csr.Root_ContentSnippetID = cs.Root_ContentSnippetID
	INNER JOIN (SELECT COUNT(*) VersionCount, Root_ContentSnippetID 
				FROM [dbo].carrot_ContentSnippet
				GROUP BY Root_ContentSnippetID 
				) csvh on csr.Root_ContentSnippetID = csvh.Root_ContentSnippetID


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER VIEW [dbo].[vw_carrot_TagCounted]
AS 

SELECT cc.ContentTagID, cc.SiteID, cc.TagText, cc.TagSlug, cc.IsPublic, ISNULL(cc2.TheCount, 0) AS UseCount
FROM dbo.carrot_ContentTag AS cc 
LEFT JOIN
      (SELECT ContentTagID, COUNT(Root_ContentID) AS TheCount
        FROM dbo.carrot_TagContentMapping
        GROUP BY ContentTagID) AS cc2 ON cc.ContentTagID = cc2.ContentTagID


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER VIEW [dbo].[vw_carrot_TrackbackQueue]
AS 

SELECT tb.TrackbackQueueID, tb.TrackBackURL, tb.TrackBackResponse, tb.CreateDate, tb.ModifiedDate, tb.TrackedBack, c.Root_ContentID, c.PageActive, c.SiteID
FROM [dbo].carrot_TrackbackQueue AS tb
INNER JOIN [dbo].carrot_RootContent AS c ON tb.Root_ContentID = c.Root_ContentID


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER VIEW [dbo].[vw_carrot_Widget]
AS 


SELECT w.Root_WidgetID, w.Root_ContentID, w.WidgetOrder, w.PlaceholderName, w.ControlPath, w.GoLiveDate, w.RetireDate, 
	cast(case when w.RetireDate < GetUTCDate() then 1 else 0 end as bit) as IsRetired,
	cast(case when w.GoLiveDate > GetUTCDate() then 1 else 0 end as bit) as IsUnReleased,
	w.WidgetActive, wd.WidgetDataID, wd.IsLatestVersion, wd.EditDate, wd.ControlProperties, cr.SiteID
FROM [dbo].carrot_Widget AS w 
INNER JOIN [dbo].carrot_WidgetData AS wd ON w.Root_WidgetID = wd.Root_WidgetID 
INNER JOIN [dbo].carrot_RootContent AS cr ON w.Root_ContentID = cr.Root_ContentID


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[carrot_BlogDateFilenameUpdate]
    @SiteID uniqueidentifier
    
/*

exec [carrot_BlogDateFilenameUpdate] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0'

*/    
    
AS BEGIN

SET NOCOUNT ON

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0
    
    DECLARE @DatePattern nvarchar(50)
    SELECT  @DatePattern = (select top 1 ct.Blog_DatePattern from dbo.carrot_Sites (nolock) ct where ct.SiteID = @SiteID)

	DECLARE @blogType uniqueidentifier
	SELECT  @blogType = (select top 1 ct.ContentTypeID from dbo.carrot_ContentType (nolock) ct where ct.ContentTypeValue = 'BlogEntry')

	DECLARE @tblTimeSlugs TABLE(
		GoLiveDateLocal datetime,
		URLBase nvarchar(256)
	)

	insert into @tblTimeSlugs(GoLiveDateLocal)
		select distinct rc.GoLiveDateLocal
		from dbo.[carrot_RootContent] as rc (nolock)
		where rc.SiteID = @SiteID
			and rc.ContentTypeID = @blogType

	IF (@DatePattern = 'yyyy/M/d' ) BEGIN
		update @tblTimeSlugs
		set URLBase = REPLACE(CONVERT(NVARCHAR(20), GoLiveDateLocal, 111), '/0', '/')
	END

	IF (@DatePattern = 'yyyy/MM' ) BEGIN
		update @tblTimeSlugs
		set URLBase = SUBSTRING(CONVERT(NVARCHAR(20), GoLiveDateLocal, 111), 1, 7)
	END

	IF (@DatePattern = 'yyyy/MMMM' ) BEGIN
		update @tblTimeSlugs
		set URLBase = CAST(YEAR(GoLiveDateLocal) as nvarchar(20)) +'/'+ DATENAME(MONTH, GoLiveDateLocal)
	END

	IF (@DatePattern = 'yyyy' ) BEGIN
		update @tblTimeSlugs
		set URLBase = CAST(YEAR(GoLiveDateLocal) as nvarchar(20))
	END

	IF (ISNULL(@DatePattern, 'yyyy/MM/dd') = 'yyyy/MM/dd' ) 
			OR EXISTS(select * from @tblTimeSlugs where URLBase is null or len(URLBase) < 1) BEGIN
		update @tblTimeSlugs
		set URLBase = CONVERT(NVARCHAR(20), GoLiveDateLocal, 111)
	END

    IF ( @@TRANCOUNT = 0 ) BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END ELSE
        SET @TranStarted = 0

		update rc
		set [FileName] = replace('/'+ s.URLBase +'/' + ISNULL(rc.PageSlug, cast(Root_ContentID as nvarchar(64))) , '//','/')
		from dbo.[carrot_RootContent] rc
			join @tblTimeSlugs s on rc.GoLiveDateLocal = s.GoLiveDateLocal
		where rc.SiteID = @SiteID
			AND rc.ContentTypeID = @blogType

    IF ( @@ERROR <> 0 ) BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF ( @TranStarted = 1 ) BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF ( @TranStarted = 1 ) BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[carrot_BlogMonthlyTallies]
    @SiteID uniqueidentifier,
    @ActiveOnly bit,    
    @TakeTop int = 10

/*

exec [carrot_BlogMonthlyTallies] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0', 1, 16

exec [carrot_BlogMonthlyTallies] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0', 0, 16

*/

AS BEGIN

SET NOCOUNT ON

	DECLARE @UTCDateTime Datetime
	SET @UTCDateTime = GetUTCDate()
	
	DECLARE @blogType uniqueidentifier
	SELECT  @blogType = (select top 1 ct.ContentTypeID from dbo.carrot_ContentType (nolock) ct where ct.ContentTypeValue = 'BlogEntry')

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
				FROM [dbo].[carrot_RootContent] (nolock)
				WHERE SiteID = @SiteID
					AND (PageActive = 1 OR @ActiveOnly = 0)
					AND (GoLiveDate < @UTCDateTime OR @ActiveOnly = 0)
					AND (RetireDate > @UTCDateTime OR @ActiveOnly = 0)
					AND ContentTypeID = @blogType ) AS Y) AS Z

		GROUP BY SiteID, DateMonth, DateSlug
		ORDER BY DateMonth DESC

	SELECT * FROM @tblTallies WHERE RowID <= @TakeTop ORDER BY RowID

    RETURN(0)

END


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[carrot_UpdateGoLiveLocal]
    @SiteID uniqueidentifier,
    @xmlDocument xml = '<rows />'
AS BEGIN

SET NOCOUNT ON

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF ( @@TRANCOUNT = 0 ) BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END ELSE BEGIN
        SET @TranStarted = 0
	END

		DECLARE @blogType uniqueidentifier
		SELECT  @blogType = (select top 1 ct.ContentTypeID from dbo.carrot_ContentType (nolock) ct where ct.ContentTypeValue = 'BlogEntry')

		DECLARE @tblContent TABLE
		(
		  GoLiveDate datetime,
		  GoLiveDateLocal datetime
		)

		DECLARE @tblBlogs TABLE
		(
		  GoLiveDate datetime,
		  GoLiveDateLocal datetime,
		  PostPrefix nvarchar(256)  
		)

		INSERT INTO @tblContent(GoLiveDate, GoLiveDateLocal)
		SELECT
			ref.value ('GoLiveDate[1]', 'datetime') as GoLiveDate,
			ref.value ('GoLiveDateLocal[1]', 'datetime') as GoLiveDateLocal
		FROM @xmlDocument.nodes ('//ContentLocalTime') T(ref);

		INSERT INTO @tblBlogs(GoLiveDate, GoLiveDateLocal, PostPrefix)
		SELECT
			ref.value ('GoLiveDate[1]', 'datetime') as GoLiveDate,
			ref.value ('GoLiveDateLocal[1]', 'datetime') as GoLiveDateLocal,
			ref.value ('PostPrefix[1]', 'nvarchar(256)') as PostPrefix
		FROM @xmlDocument.nodes ('//BlogPostPageUrl') T(ref);

		update @tblBlogs
			set PostPrefix = cast(DATEPART(YEAR, GoLiveDateLocal) as varchar(32)) + '/' + cast(DATEPART(MONTH, GoLiveDateLocal) as varchar(32)) + '/' + cast(DATEPART(DAY, GoLiveDateLocal) as varchar(32)) + '/'
		where PostPrefix is null or len(PostPrefix) < 3

		UPDATE rc
			SET GoLiveDateLocal = c.GoLiveDateLocal
		FROM [dbo].[carrot_RootContent] rc
			JOIN @tblContent c on rc.GoLiveDate = c.GoLiveDate
		WHERE rc.SiteID = @SiteID

		UPDATE rc
			SET [FileName] = replace(b.PostPrefix + '/' + rc.PageSlug, '//',  '/')
		FROM [dbo].[carrot_RootContent] rc
			JOIN @tblBlogs b on rc.GoLiveDate = b.GoLiveDate
		WHERE rc.SiteID = @SiteID 
				and rc.ContentTypeID = @blogType

	IF ( @@ERROR <> 0 ) BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF ( @TranStarted = 1 ) BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF ( @TranStarted = 1 ) BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END


GO
