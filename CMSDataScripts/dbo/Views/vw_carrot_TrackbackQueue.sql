CREATE VIEW [dbo].[vw_carrot_TrackbackQueue]
AS 

SELECT tb.TrackbackQueueID, tb.TrackBackURL, tb.TrackBackResponse, tb.CreateDate, tb.ModifiedDate, tb.TrackedBack, c.Root_ContentID, c.PageActive, c.SiteID
FROM [dbo].carrot_TrackbackQueue AS tb
INNER JOIN [dbo].carrot_RootContent AS c ON tb.Root_ContentID = c.Root_ContentID