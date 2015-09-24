CREATE VIEW [dbo].[vw_carrot_EditorURL]
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