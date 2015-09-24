CREATE PROCEDURE [dbo].[carrot_BlogDateFilenameUpdate]
    @SiteID uniqueidentifier
    
/*

exec [carrot_BlogDateFilenameUpdate] '3BD253EA-AC65-4eb6-A4E7-BB097C2255A0' 

*/    
    
AS BEGIN

SET NOCOUNT ON

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0
    
    DECLARE @DatePattern nvarchar(50)
    SELECT  @DatePattern = (select top 1 ct.Blog_DatePattern from dbo.carrot_Sites ct where ct.SiteID = @SiteID)

    DECLARE @ContentTypeID uniqueidentifier
    SELECT  @ContentTypeID = (select top 1 ct.ContentTypeID from dbo.carrot_ContentType ct where ct.ContentTypeValue = 'BlogEntry')

	DECLARE @tblTimeSlugs TABLE(
		Root_ContentID uniqueidentifier,
		GoLiveDateLocal datetime not null,
		TempSlug nvarchar(128) not null,
		URLBase nvarchar(50) null
	)

	insert into @tblTimeSlugs(Root_ContentID, GoLiveDateLocal, TempSlug)
		select rc.Root_ContentID, rc.GoLiveDateLocal,  '/' + LOWER(CAST(rc.Root_ContentID as nvarchar(60)) + '.aspx') as tmpSlug 
		from dbo.[carrot_RootContent] as rc
		where rc.SiteID = @SiteID
			AND rc.ContentTypeID = @ContentTypeID


	IF (ISNULL(@DatePattern, 'yyyy/MM/dd') = 'yyyy/MM/dd' ) BEGIN
		update @tblTimeSlugs
		set URLBase = CONVERT(NVARCHAR(20), GoLiveDateLocal, 111)
	END

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


    IF ( @@TRANCOUNT = 0 ) BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END ELSE
        SET @TranStarted = 0

		--select replace('/'+ s.URLBase +'/' + ISNULL(PageSlug, s.TempSlug), '//','/') as FN
		update rc
		set [FileName] = replace('/'+ s.URLBase +'/' + ISNULL(rc.PageSlug, s.TempSlug) , '//','/')
		from dbo.[carrot_RootContent] rc
			join @tblTimeSlugs s on rc.Root_ContentID = s.Root_ContentID
		where rc.SiteID = @SiteID
			AND rc.ContentTypeID = @ContentTypeID


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