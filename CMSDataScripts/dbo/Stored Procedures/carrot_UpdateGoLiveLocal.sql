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

		DECLARE @tblContent TABLE
		(
		  Root_ContentID uniqueidentifier,
		  GoLiveDateLocal datetime
		)

		DECLARE @tblBlogs TABLE
		(
		  Root_ContentID uniqueidentifier,
		  GoLiveDateLocal datetime,
		  [FileName] nvarchar(256)  
		)

		INSERT INTO @tblContent(Root_ContentID, GoLiveDateLocal)
		SELECT
			ref.value ('Root_ContentID[1]', 'uniqueidentifier') as Root_ContentID,
			ref.value ('GoLiveDateLocal[1]', 'datetime') as GoLiveDateLocal
		FROM @xmlDocument.nodes ('//ContentLocalTime') T(ref);

		INSERT INTO @tblBlogs(Root_ContentID, GoLiveDateLocal, [FileName])
		SELECT
			ref.value ('Root_ContentID[1]', 'uniqueidentifier') as Root_ContentID,
			ref.value ('GoLiveDateLocal[1]', 'datetime') as GoLiveDateLocal,	
			ref.value ('FileName[1]', 'nvarchar(256)') as [FileName]
		FROM @xmlDocument.nodes ('//BlogPostPageUrl') T(ref);

		UPDATE rc
			SET GoLiveDateLocal = c.GoLiveDateLocal
		FROM [dbo].[carrot_RootContent] rc
			INNER JOIN @tblContent c on rc.Root_ContentID = c.Root_ContentID
		WHERE SiteID = @SiteID

		UPDATE rc
			SET [FileName] = b.[FileName]
		FROM [dbo].[carrot_RootContent] rc
			INNER JOIN @tblBlogs b on rc.Root_ContentID = b.Root_ContentID
		WHERE SiteID = @SiteID


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