CREATE VIEW [dbo].[vw_carrot_Comment]
AS 

SELECT cc.ContentCommentID, cc.CreateDate, cc.CommenterIP, cc.CommenterName, cc.CommenterEmail, cc.CommenterURL, cc.PostComment, cc.IsApproved, cc.IsSpam, 
	c.Root_ContentID, c.SiteID, c.[FileName], c.PageHead, c.TitleBar, c.NavMenuText, c.IsRetired, c.IsUnReleased, c.RetireDate, c.GoLiveDate, 
	c.PageActive, c.ShowInSiteNav, c.ShowInSiteMap, c.BlockIndex, c.ContentTypeID, c.ContentTypeValue
FROM  dbo.carrot_ContentComment AS cc 
	INNER JOIN dbo.vw_carrot_Content AS c ON cc.Root_ContentID = c.Root_ContentID
WHERE (c.IsLatestVersion = 1)