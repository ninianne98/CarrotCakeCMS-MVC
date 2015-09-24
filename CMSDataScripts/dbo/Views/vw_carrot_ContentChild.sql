CREATE VIEW [dbo].[vw_carrot_ContentChild]
AS 

SELECT DISTINCT cc.SiteID, cc.Root_ContentID, cc.[FileName], 
          cc.RetireDate, cc.GoLiveDate, 
          cc.IsRetired, cc.IsUnReleased, 
          cp.Root_ContentID as Parent_ContentID, cp.[FileName] AS ParentFileName,
          cp.RetireDate AS ParentRetireDate, cp.GoLiveDate AS ParentGoLiveDate, 
          cp.IsRetired as IsParentRetired, cp.IsUnReleased as IsParentUnReleased
FROM dbo.vw_carrot_Content AS cc 
	INNER JOIN dbo.vw_carrot_Content AS cp ON cc.Parent_ContentID = cp.Root_ContentID
WHERE cp.IsLatestVersion = 1 AND cc.IsLatestVersion = 1
	AND cc.SiteID = cp.SiteID