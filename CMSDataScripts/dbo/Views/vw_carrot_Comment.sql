CREATE VIEW [dbo].[vw_carrot_Comment]
AS 

SELECT cc.ContentCommentID, cc.CreateDate, cc.CommenterIP, cc.CommenterName, cc.CommenterEmail, cc.CommenterURL, cc.PostComment, cc.IsApproved, cc.IsSpam, 
	c.Root_ContentID, c.SiteID, c.[FileName], c.PageHead, c.TitleBar, c.NavMenuText, c.ContentTypeID, c.IsRetired, c.IsUnReleased, c.RetireDate, c.GoLiveDate
FROM [dbo].carrot_ContentComment AS cc 
	INNER JOIN [dbo].vw_carrot_Content AS c on cc.Root_ContentID = c.Root_ContentID
WHERE c.IsLatestVersion = 1