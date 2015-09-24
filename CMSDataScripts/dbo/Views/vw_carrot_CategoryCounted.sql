CREATE VIEW [dbo].[vw_carrot_CategoryCounted]
AS 

SELECT cc.ContentCategoryID, cc.SiteID, cc.CategoryText, cc.CategorySlug, cc.IsPublic, ISNULL(cc2.TheCount, 0) AS UseCount
FROM dbo.carrot_ContentCategory AS cc 
LEFT JOIN
      (SELECT ContentCategoryID, COUNT(Root_ContentID) AS TheCount
        FROM dbo.carrot_CategoryContentMapping
        GROUP BY ContentCategoryID) AS cc2 ON cc.ContentCategoryID = cc2.ContentCategoryID