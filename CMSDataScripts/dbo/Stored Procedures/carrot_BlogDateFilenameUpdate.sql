
CREATE PROCEDURE [dbo].[carrot_BlogDateFilenameUpdate]
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