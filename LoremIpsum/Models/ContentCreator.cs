using CarrotCake.CMS.Plugins.LoremIpsum.Code;
using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CarrotCake.CMS.Plugins.LoremIpsum.Models {

	public class ContentCreator {
		protected List<string> _titleCache = new List<string>();

		public ContentCreator() {
			this.PageLinks = new List<string>();
			this.HowMany = 10;
			this.BlogComments = 0;
			this.Categories = 0;
			this.Tags = 0;

			this.TopLevel = false;
			this.DateFrom = DateTime.Now.AddDays(-90).Date;
			this.DateTo = DateTime.Now.AddDays(3).Date;
		}

		public ContentCreator(ContentPageType.PageType contentType) : this() {
			this.ContentType = contentType;

			if (contentType == ContentPageType.PageType.ContentEntry) {
				this.TopLevel = true;
			}

			if (contentType == ContentPageType.PageType.BlogEntry) {
				this.HowMany = 25;
				this.Categories = 4;
				this.Tags = 2;
			}
		}

		public ContentPageType.PageType ContentType { get; set; }
		public int HowMany { get; set; }
		public int Categories { get; set; }
		public int Tags { get; set; }
		public int BlogComments { get; set; }

		public bool TopLevel { get; set; }

		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }

		public List<string> PageLinks { get; set; }

		protected List<string> GetLines() {
			var lst = new List<string>();

			var txt = WebHelper.ReadEmbededScript("CarrotCake.CMS.Plugins.LoremIpsum.Code.lorem.txt").Replace("\r\n", "\n");

			lst = txt.Split('\n').Where(x => x.Length > 3).OrderBy(x => Guid.NewGuid())
				.Where(x => !_titleCache.Contains(x.ToLowerInvariant()))
				.Select(x => x.Trim()).Take(25).ToList();

			if (_titleCache.Count >= 50) {
				_titleCache = _titleCache.Take(25).ToList();
			}

			_titleCache.Add(lst[0].ToLowerInvariant());

			return lst;
		}

		protected List<string> GetWords() {
			var lines = GetLines();
			return (string.Join(" ", lines.Take(12))).Replace(".", " ").Replace(",", " ")
						.Replace("  ", " ").Replace("  ", " ").ToLowerInvariant()
						.Split(' ').OrderBy(x => Guid.NewGuid())
						.Where(x => x.Length >= 5).Select(x => x.Trim()).Distinct().ToList();
		}

		protected string GetXWords(string line, int words) {
			return string.Join(" ", line.Replace(".", " ").Replace(",", " ").Replace("  ", " ").Replace("  ", " ")
						.Split(' ').Where(x => x.Length > 0)
						.Take(words).Select(x => x.Trim()));
		}

		protected string BuildPP(List<string> list, int[] rows) {
			var p = new HtmlTag("p");
			p.InnerHtml = string.Empty;

			foreach (var r in rows) {
				p.InnerHtml = p.InnerHtml + "  " + list[r];
			}

			return p.ToString().Replace("<p >", "<p>") + Environment.NewLine;
		}

		protected void SeedContent(ContentPageType.PageType pageType) {
			_titleCache = new List<string>();
			SiteData site = SiteData.CurrentSite;
			var textinfo = CultureInfo.CurrentCulture.TextInfo;

			if (this.HowMany <= 0) {
				this.HowMany = 1;
			}
			if (this.BlogComments <= 0) {
				this.BlogComments = 1;
			}
			if (this.Categories <= 0) {
				this.Categories = 0;
			}
			if (this.Tags <= 0) {
				this.Tags = 0;
			}

			var dtSite = DateTime.UtcNow.Date.AddHours(8).AddMinutes(15);
			var tagIds = new List<Guid>();
			var catIds = new List<Guid>();

			var backdate = (dtSite.Date - this.DateFrom.Date).TotalDays;
			var days = (this.DateTo.Date - this.DateFrom.Date).TotalDays - 1;

			var incDateBy = (days / (1.0 * this.HowMany)) + 0.01;
			var dateOffset = backdate * -1.0;

			using (var navHelper = new SiteNavHelperReal()) {
				for (int p = 1; p <= this.HowMany; p++) {
					var home = navHelper.FindHome(site.SiteID, false);

					var pageContents = new ContentPage();
					pageContents.ContentType = pageType;
					pageContents.ContentID = Guid.NewGuid();
					pageContents.Root_ContentID = Guid.NewGuid();
					pageContents.SiteID = site.SiteID;
					pageContents.TemplateFile = SiteData.DefaultTemplateFilename;
					pageContents.IsLatestVersion = true;
					pageContents.PageActive = true;
					pageContents.BlockIndex = false;
					pageContents.EditDate = site.Now;
					pageContents.EditUserId = SecurityData.CurrentUserGuid;

					if (pageContents.ContentType == ContentPageType.PageType.BlogEntry) {
						tagIds = site.GetTagList().Where(x => x.IsPublic).OrderBy(x => Guid.NewGuid()).Select(x => x.ContentTagID).Take(this.Tags).ToList();
						catIds = site.GetCategoryList().Where(x => x.IsPublic).OrderBy(x => Guid.NewGuid()).Select(x => x.ContentCategoryID).Take(this.Categories).ToList();
					}

					if (pageContents.ContentType == ContentPageType.PageType.BlogEntry) {
						pageContents.GoLiveDate = CMSConfigHelper.CalcNearestFiveMinTime(dtSite.AddMinutes(-7).AddDays(dateOffset));
						pageContents.RetireDate = pageContents.GoLiveDate.AddYears(200);
						dateOffset = dateOffset + incDateBy;

						pageContents.NavOrder = SiteData.BlogSortOrderNumber;
						pageContents.ShowInSiteNav = false;
						pageContents.ShowInSiteMap = false;
					}

					if (pageContents.ContentType == ContentPageType.PageType.ContentEntry) {
						var pages = navHelper.GetTopNavigation(site.SiteID, false);

						if (home == null || this.TopLevel) {
							pageContents.Parent_ContentID = null;
						}

						if (pages.Count >= 6 || (home != null && !this.TopLevel)) {
							// once top level is built out, build second level,
							// but only go to third level if there are a high number of pages
							pages = navHelper.GetTwoLevelNavigation(site.SiteID, false);
							if (pages.Count <= 20) {
								pages = pages.Where(x => x.Parent_ContentID == null).ToList();
							}

							var parentPage = pages.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
							pageContents.Parent_ContentID = parentPage.Root_ContentID;
						}

						pageContents.GoLiveDate = CMSConfigHelper.CalcNearestFiveMinTime(dtSite.AddDays(-7));
						pageContents.RetireDate = pageContents.GoLiveDate.AddYears(200);

						pageContents.NavOrder = navHelper.GetSitePageCount(pageContents.SiteID, pageContents.ContentType) * 2;
						pageContents.ShowInSiteNav = true;
						pageContents.ShowInSiteMap = true;
					}

					var lines = GetLines();
					var fullTitle = textinfo.ToTitleCase(GetXWords(lines[0], 12));
					var shortTitle = textinfo.ToTitleCase(GetXWords(lines[0], 6));

					var txt = BuildPP(lines, new int[] { 1, 2, 3, 13 })
						+ BuildPP(lines, new int[] { 4, 5, 6, 7, 14 })
						+ BuildPP(lines, new int[] { 8, 9, 10, 11, 12 })
						+ BuildPP(lines, new int[] { 15, 16, 17, 18 });

					pageContents.TitleBar = fullTitle;
					pageContents.PageHead = fullTitle;

					if (pageContents.ContentType == ContentPageType.PageType.BlogEntry) {
						pageContents.NavMenuText = GetXWords(fullTitle, 5);
					}
					if (pageContents.ContentType == ContentPageType.PageType.ContentEntry) {
						pageContents.MetaKeyword = lines[19];
						pageContents.MetaDescription = lines[20];
						pageContents.NavMenuText = GetXWords(fullTitle, 2);
					}

					pageContents.PageText = txt;
					pageContents.LeftPageText = string.Empty;
					pageContents.RightPageText = string.Empty;

					if (pageContents.ContentType == ContentPageType.PageType.BlogEntry) {
						pageContents.PageSlug = SiteData.GenerateNewFilename(pageContents.Root_ContentID, shortTitle, pageContents.GoLiveDate, pageContents.ContentType);
						pageContents.FileName = ContentPageHelper.CreateFileNameFromSlug(site, pageContents.GoLiveDate, pageContents.PageSlug);
					}
					if (pageContents.ContentType == ContentPageType.PageType.ContentEntry) {
						pageContents.PageSlug = null;
						pageContents.FileName = SiteData.GenerateNewFilename(pageContents.Root_ContentID, shortTitle, pageContents.GoLiveDate, pageContents.ContentType);
					}

					if (pageContents.ContentType == ContentPageType.PageType.BlogEntry) {
						List<ContentCategory> lstCat = (from l in site.GetCategoryList()
														where catIds.Contains(l.ContentCategoryID)
														select l).ToList();
						List<ContentTag> lstTag = (from l in site.GetTagList()
												   where tagIds.Contains(l.ContentTagID)
												   select l).ToList();

						pageContents.ContentCategories = lstCat;
						pageContents.ContentTags = lstTag;
					}

					this.PageLinks.Add(pageContents.FileName);

					pageContents.SavePageEdit();

					if (pageType == ContentPageType.PageType.BlogEntry
								&& pageContents.GoLiveDate < site.Now
								&& this.BlogComments > 0) {
						var textinfo2 = new CultureInfo("en-US", false).TextInfo;

						for (int bc = 0; bc < this.BlogComments; bc++) {
							var msgtxt = GetLines();
							var words = GetWords();

							var name = textinfo2.ToTitleCase(string.Format("{0} {1}", words[1], words[2]));
							var eml = string.Format("{0}.{1}@{2}.com", words[5], words[6], words[7]).ToLowerInvariant();
							var url = words.Count > 12 ? string.Format("http://www.{0}-{1}.com/", words[10], words[11]).ToLowerInvariant() : string.Empty;

							var pc = new PostComment();
							pc.ContentCommentID = Guid.NewGuid();
							pc.CreateDate = pageContents.GoLiveDate.AddMinutes(5).AddDays(bc).AddMinutes(bc * 2.0);
							pc.Root_ContentID = pageContents.Root_ContentID;
							pc.CommenterIP = "127.0.0.1";
							pc.IsApproved = true;
							pc.IsSpam = false;
							pc.PostCommentText = HttpUtility.HtmlEncode(msgtxt[1] + " " + msgtxt[2] + " " + msgtxt[3]);
							pc.CommenterName = HttpUtility.HtmlEncode(name);
							pc.CommenterEmail = HttpUtility.HtmlEncode(eml ?? string.Empty);
							pc.CommenterURL = HttpUtility.HtmlEncode(url ?? string.Empty);

							if (pc.CreateDate < site.Now) {
								pc.Save();
							}
						}
					}
				}
			}
		}

		public void BuildPages() {
			SeedContent(ContentPageType.PageType.ContentEntry);
		}

		public void BuildPosts() {
			var textinfo = new CultureInfo("en-US", false).TextInfo;
			SiteData site = SiteData.CurrentSite;

			List<string> tags = site.GetTagList().Select(x => x.TagSlug.ToLowerInvariant()).ToList();
			List<string> cats = site.GetCategoryList().Select(x => x.CategorySlug.ToLowerInvariant()).ToList();

			if (cats.Count < 25) {
				var newCats = new List<ContentCategory>();

				var words = GetWords();

				for (int i = 0; i < (words.Count - 1); i++) {
					var s = words[i].Trim() + " " + words[i + 1].Trim();

					var item = new ContentCategory();
					item.SiteID = site.SiteID;
					item.ContentCategoryID = Guid.NewGuid();
					item.CategorySlug = ContentPageHelper.ScrubSlug(s);
					item.CategoryText = textinfo.ToTitleCase(s.Trim());
					item.IsPublic = true;
					newCats.Add(item);

					i++;
				}

				newCats.RemoveAll(x => cats.Contains(x.CategorySlug.ToLowerInvariant()));
				foreach (var v in newCats.Take(25)) {
					v.Save();
				}
			}

			if (tags.Count < 15) {
				var newTags = new List<ContentTag>();

				var words = GetWords();

				foreach (var s in words) {
					var item = new ContentTag();
					item.SiteID = site.SiteID;
					item.ContentTagID = Guid.NewGuid();
					item.TagSlug = ContentPageHelper.ScrubSlug(s.Trim());
					item.TagText = textinfo.ToTitleCase(s.Trim());
					item.IsPublic = true;
					newTags.Add(item);
				}

				newTags.RemoveAll(x => tags.Contains(x.TagSlug.ToLowerInvariant()));
				foreach (var v in newTags.Take(15)) {
					v.Save();
				}
			}

			SeedContent(ContentPageType.PageType.BlogEntry);
		}
	}
}