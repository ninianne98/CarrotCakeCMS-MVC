
CREATE VIEW [dbo].[vw_carrot_ContentSnippet]
AS 

SELECT csr.Root_ContentSnippetID, csr.SiteID, csr.ContentSnippetName, csr.ContentSnippetSlug, csr.CreateUserId, csr.CreateDate, 
	csr.ContentSnippetActive, cs.ContentSnippetID, cs.IsLatestVersion, cs.EditUserId, cs.EditDate, cs.ContentBody, 
	csr.Heartbeat_UserId, csr.EditHeartbeat, csr.GoLiveDate, csr.RetireDate,
	cast(case when csr.RetireDate < GetUTCDate() then 1 else 0 end as bit) as IsRetired,
	cast(case when csr.GoLiveDate > GetUTCDate() then 1 else 0 end as bit) as IsUnReleased,
	csvh.VersionCount
FROM carrot_RootContentSnippet AS csr 
	INNER JOIN carrot_ContentSnippet AS cs ON csr.Root_ContentSnippetID = cs.Root_ContentSnippetID
	INNER JOIN (SELECT COUNT(*) VersionCount, Root_ContentSnippetID 
				FROM [dbo].carrot_ContentSnippet
				GROUP BY Root_ContentSnippetID 
				) csvh on csr.Root_ContentSnippetID = csvh.Root_ContentSnippetID