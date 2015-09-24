CREATE VIEW [dbo].[vw_carrot_CategoryURL]
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