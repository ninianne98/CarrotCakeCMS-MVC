
CREATE PROCEDURE [dbo].[carrot_UpdateGoLiveLocal]
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