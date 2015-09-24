CREATE VIEW [dbo].[vw_carrot_TagURL]
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