using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	public class ListInBody : ITextBodyUpdate {

		public string UpdateContent(string textContent) {
			if (!string.IsNullOrEmpty(textContent)) {
				var nav = FindMatches(textContent);

				if (nav.Any()) {
					var sb = new StringBuilder(textContent);
					foreach (var n in nav) {
						sb.Replace(n.MatchedString, n.ReplaceString);
					}
					textContent = sb.ToString();
				}
			}

			return textContent;
		}

		public string UpdateContentPlainText(string textContent) {
			return UpdateContent(textContent);
		}

		public string UpdateContentRichText(string textContent) {
			return UpdateContent(textContent);
		}

		public string UpdateContentComment(string textContent) {
			return UpdateContent(textContent);
		}

		public string UpdateContentSnippet(string textContent) {
			return UpdateContent(textContent);
		}

		protected List<NavReplace> FindMatches(string strIn) {
			var nav = new List<NavReplace>();

			List<NavMatch> matches = Regex.Matches(strIn.Replace(Environment.NewLine, " "), @"\[carrotnav([^]]*)\]")
								   .Cast<Match>()
								   .Select(x => new NavMatch(x.Groups[0].Value, x.Groups[1].Value))
								   .ToList();

			foreach (var match in matches) {
				var nr = new NavReplace();
				nr.MatchedString = match.Match0;
				bool ignore = false;

				//create a fake html/xml node based on the internals of the match
				var xml = string.Format("<div {0} ></div>", match.Match1);

				try {
					var list = new SortableList();
					var elm = XElement.Parse(xml);

					var listType = "recent-blog";
					var sortdir = "desc";
					int take = 5;

					try { listType = elm.Attribute("list").Value ?? "recent-blog"; } catch { }
					try { sortdir = elm.Attribute("order").Value ?? "desc"; } catch { }
					try { take = int.Parse(elm.Attribute("take").Value ?? "5"); } catch { }
					try { ignore = Convert.ToBoolean(elm.Attribute("ignore").Value.ToLowerInvariant() == "true"); } catch { }

					list.SortNavBy = sortdir.ToUpperInvariant() == "ASC"
										? SimpleListSortable.SortOrder.TitleAsc
											: SimpleListSortable.SortOrder.TitleDesc;

					if (listType.ToLowerInvariant().StartsWith("recent")) {
						using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
							if (listType.ToLowerInvariant() == "recent-blog") {
								list.NavigationData = navHelper.GetLatestPosts(SiteData.CurrentSiteID, take, !SecurityData.IsAuthEditor);
							}
							if (listType.ToLowerInvariant() == "recent-page") {
								list.NavigationData = navHelper.GetLatest(SiteData.CurrentSiteID, take, !SecurityData.IsAuthEditor);
							}
						}
					}

					if (listType.ToLowerInvariant() == "top") {
						list.NavigationData = list.CmsPage.TopNav;
					}
					if (listType.ToLowerInvariant() == "child") {
						list.NavigationData = list.CmsPage.ChildNav;
					}
					if (listType.ToLowerInvariant() == "sibling") {
						list.NavigationData = list.CmsPage.SiblingNav;
					}

					list.LoadData();

					var html = list.ToHtmlString();
#if DEBUG
					// when in debug, show the original, too
					html = string.Format("{0} \r\n<br />\r\n {1}", nr.MatchedString, html);
#endif
					nr.ReplaceString = html;
				} catch (Exception ex) {
					nr.ReplaceString = "ERROR: " + match.Match0;
				}

				// provide ignore flag to bypass logic
				if (!ignore) {
					nav.Add(nr);
				}
			}

			return nav;
		}

		//==================
		public class NavReplace {
			public string MatchedString { get; set; }
			public string ReplaceString { get; set; }
		}

		public class NavMatch {

			public NavMatch() { }

			public NavMatch(string m0, string m1) {
				this.Match0 = m0;
				this.Match1 = m1;
			}

			public string Match0 { get; set; }
			public string Match1 { get; set; }
		}
	}
}