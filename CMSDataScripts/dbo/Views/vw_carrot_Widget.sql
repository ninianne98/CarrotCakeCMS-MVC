

CREATE VIEW [dbo].[vw_carrot_Widget]
AS 


SELECT w.Root_WidgetID, w.Root_ContentID, w.WidgetOrder, w.PlaceholderName, w.ControlPath, w.GoLiveDate, w.RetireDate, 
	cast(case when w.RetireDate < GetUTCDate() then 1 else 0 end as bit) as IsRetired,
	cast(case when w.GoLiveDate > GetUTCDate() then 1 else 0 end as bit) as IsUnReleased,
	w.WidgetActive, wd.WidgetDataID, wd.IsLatestVersion, wd.EditDate, wd.ControlProperties, cr.SiteID
FROM [dbo].carrot_Widget AS w 
INNER JOIN [dbo].carrot_WidgetData AS wd ON w.Root_WidgetID = wd.Root_WidgetID 
INNER JOIN [dbo].carrot_RootContent AS cr ON w.Root_ContentID = cr.Root_ContentID