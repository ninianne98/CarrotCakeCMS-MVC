CREATE VIEW [dbo].[vw_carrot_EditHistory]
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