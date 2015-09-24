CREATE VIEW [dbo].[vw_carrot_TagCounted]
AS 

SELECT cc.ContentTagID, cc.SiteID, cc.TagText, cc.TagSlug, cc.IsPublic, ISNULL(cc2.TheCount, 0) AS UseCount
FROM dbo.carrot_ContentTag AS cc 
LEFT JOIN
      (SELECT ContentTagID, COUNT(Root_ContentID) AS TheCount
        FROM dbo.carrot_TagContentMapping
        GROUP BY ContentTagID) AS cc2 ON cc.ContentTagID = cc2.ContentTagID