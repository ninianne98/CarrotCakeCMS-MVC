using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class WPBlogReader {

		public WPBlogReader() { }

		public XmlDocument LoadFile(string FileName) {
			XmlDocument doc = new XmlDocument();
			doc.Load(FileName);
			return doc;
		}

		public XmlDocument LoadText(string InputString) {
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(InputString);
			return doc;
		}

		public WordPressSite GetAllData(XmlDocument doc) {
			WordPressSite site = new WordPressSite();

			List<WordPressPost> lstWPP = new List<WordPressPost>();
			List<WordPressComment> lstWPC = new List<WordPressComment>();
			List<WordPressUser> lstAuth = new List<WordPressUser>();

			XmlNode rssNode = doc.SelectSingleNode("//rss");

			XmlNamespaceManager rssNamespace = new XmlNamespaceManager(doc.NameTable);

			foreach (XmlAttribute attrib in rssNode.Attributes) {
				if (attrib != null && attrib.Value.ToLowerInvariant().StartsWith("http")) {
					rssNamespace.AddNamespace(attrib.LocalName, attrib.Value);
				}
			}

			site.SiteTitle = rssNode.SelectSingleNode("channel/title").InnerText;
			site.SiteDescription = rssNode.SelectSingleNode("channel/description").InnerText;
			site.SiteURL = rssNode.SelectSingleNode("channel/link").InnerText;
			site.ImportSource = rssNode.SelectSingleNode("channel/generator").InnerText;
			site.ExtractDate = Convert.ToDateTime(rssNode.SelectSingleNode("channel/pubDate").InnerText);
			site.wxrVersion = rssNode.SelectSingleNode("channel/wp:wxr_version", rssNamespace).InnerText;

			site.Categories = new List<InfoKVP>();

			XmlNodeList catNodes = doc.SelectNodes("//rss/channel/wp:category", rssNamespace);
			foreach (XmlNode node in catNodes) {
				string slug = node.SelectSingleNode("wp:category_nicename", rssNamespace).InnerText;
				string title = node.SelectSingleNode("wp:cat_name", rssNamespace).InnerText;
				site.Categories.Add(new InfoKVP(slug, title));
			}
			catNodes = doc.SelectNodes("//rss/channel/item/category[@domain='category']");
			foreach (XmlNode node in catNodes) {
				if (node.Attributes["nicename"] != null) {
					string slug = node.Attributes["nicename"].InnerText;
					string title = node.InnerText;
					site.Categories.Add(new InfoKVP(slug, title));
				}
			}

			site.Tags = new List<InfoKVP>();

			XmlNodeList tagNodes = doc.SelectNodes("//rss/channel/wp:tag", rssNamespace);
			foreach (XmlNode node in tagNodes) {
				string slug = node.SelectSingleNode("wp:tag_slug", rssNamespace).InnerText;
				string title = node.SelectSingleNode("wp:tag_name", rssNamespace).InnerText;
				site.Tags.Add(new InfoKVP(slug, title));
			}
			tagNodes = doc.SelectNodes("//rss/channel/item/category[@domain='post_tag']");
			foreach (XmlNode node in tagNodes) {
				if (node.Attributes["nicename"] != null) {
					string slug = node.Attributes["nicename"].InnerText;
					string title = node.InnerText;
					site.Tags.Add(new InfoKVP(slug, title));
				}
			}

			XmlNodeList rssAuthors = doc.SelectNodes("//rss/channel/wp:author", rssNamespace);
			foreach (XmlNode node in rssAuthors) {
				WordPressUser wpu = new WordPressUser();
				wpu.AuthorId = int.Parse(node.SelectSingleNode("wp:author_id", rssNamespace).InnerText);
				wpu.Login = node.SelectSingleNode("wp:author_login", rssNamespace).InnerText;
				wpu.Email = node.SelectSingleNode("wp:author_email", rssNamespace).InnerText;
				try { wpu.FirstName = node.SelectSingleNode("wp:author_first_name", rssNamespace).InnerText; } catch { }
				try { wpu.LastName = node.SelectSingleNode("wp:author_last_name", rssNamespace).InnerText; } catch { }
				wpu.ImportUserID = Guid.Empty;

				lstAuth.Add(wpu);
			}

			XmlNodeList nodes = doc.SelectNodes("//rss/channel/item");

			foreach (XmlNode node in nodes) {
				WordPressPost wpp = new WordPressPost();
				wpp.PostType = WordPressPost.WPPostType.Unknown;
				wpp.IsPublished = false;
				wpp.PostOrder = 0;
				wpp.ImportRootID = Guid.NewGuid();

				wpp.PostTitle = node.SelectSingleNode("title").InnerText.Trim();
				wpp.PostName = node.SelectSingleNode("wp:post_name", rssNamespace).InnerText.Trim();

				if (String.IsNullOrEmpty(wpp.PostName)) {
					wpp.PostName = wpp.PostTitle.ToLowerInvariant();
				}
				if (String.IsNullOrEmpty(wpp.PostName)) {
					wpp.PostName = wpp.ImportRootID.ToString().ToLowerInvariant();
				}
				if (String.IsNullOrEmpty(wpp.PostTitle)) {
					wpp.PostTitle = "(No Title)";
				}

				wpp.PostName = ContentPageHelper.ScrubSlug(wpp.PostName);

				wpp.PostAuthor = node.SelectSingleNode("dc:creator", rssNamespace).InnerText;

				string postType = node.SelectSingleNode("wp:post_type", rssNamespace).InnerText;

				switch (postType) {
					case "attachment":
						wpp.PostType = WordPressPost.WPPostType.Attachment;
						break;

					case "post":
						wpp.PostType = WordPressPost.WPPostType.BlogPost;
						break;

					case "page":
						wpp.PostType = WordPressPost.WPPostType.Page;
						break;
				}

				if (wpp.PostName.Length > 200 && wpp.PostType == WordPressPost.WPPostType.Page) {
					wpp.PostName = wpp.PostName.Substring(0, 175).Trim();
				}
				if (wpp.PostName.Length > 255 && (wpp.PostType == WordPressPost.WPPostType.BlogPost || wpp.PostType == WordPressPost.WPPostType.Attachment)) {
					wpp.PostName = wpp.PostName.Substring(0, 250).Trim();
				}

				wpp.PostDateUTC = DateTime.UtcNow;
				try { wpp.PostDateUTC = Convert.ToDateTime(node.SelectSingleNode("wp:post_date", rssNamespace).InnerText); } catch { }
				try { wpp.PostDateUTC = Convert.ToDateTime(node.SelectSingleNode("wp:post_date_gmt", rssNamespace).InnerText); } catch { }

				wpp.PostContent = node.SelectSingleNode("content:encoded", rssNamespace).InnerText;

				if (node.SelectSingleNode("wp:attachment_url", rssNamespace) != null) {
					wpp.AttachmentURL = node.SelectSingleNode("wp:attachment_url", rssNamespace).InnerText;
				}

				wpp.ImportFileSlug = ContentPageHelper.ScrubFilename(wpp.ImportRootID, "/" + wpp.PostName.Trim());
				wpp.ImportFileName = ContentPageHelper.ScrubFilename(wpp.ImportRootID, wpp.ImportFileSlug);

				if (wpp.PostType == WordPressPost.WPPostType.Attachment) {
					wpp.ImportFileSlug = wpp.AttachmentURL.Substring(wpp.AttachmentURL.LastIndexOf("/")).Replace("//", "/").Trim();
					wpp.ImportFileName = wpp.ImportFileSlug;

					if (node.SelectSingleNode("excerpt:encoded", rssNamespace) != null) {
						wpp.PostTitle = node.SelectSingleNode("excerpt:encoded", rssNamespace).InnerText;
					}
					if (node.SelectSingleNode("content:encoded", rssNamespace) != null) {
						wpp.PostContent = node.SelectSingleNode("content:encoded", rssNamespace).InnerText;
					}
				}

				if (String.IsNullOrEmpty(wpp.PostContent)) {
					wpp.PostContent = "";
				}
				wpp.PostContent = wpp.PostContent.Replace("\r\n", "\n").Trim();

				wpp.ParentPostID = int.Parse(node.SelectSingleNode("wp:post_parent", rssNamespace).InnerText);
				wpp.PostID = int.Parse(node.SelectSingleNode("wp:post_id", rssNamespace).InnerText);
				wpp.PostOrder = int.Parse(node.SelectSingleNode("wp:menu_order", rssNamespace).InnerText);

				if (node.SelectSingleNode("wp:status", rssNamespace).InnerText == "publish") {
					wpp.IsPublished = true;
				}

				if (wpp.PostType == WordPressPost.WPPostType.BlogPost
					|| (wpp.PostType == WordPressPost.WPPostType.Page && wpp.ParentPostID > 0)) {
					wpp.PostOrder = wpp.PostOrder + 10;
				}

				wpp.Categories = new List<string>();
				XmlNodeList nodesCat = node.SelectNodes("category[@domain='category']");
				foreach (XmlNode n in nodesCat) {
					if (n.Attributes["nicename"] != null) {
						wpp.Categories.Add(n.Attributes["nicename"].Value);
					}
				}

				wpp.Tags = new List<string>();
				XmlNodeList nodesTag = node.SelectNodes("category[@domain='post_tag']");
				foreach (XmlNode n in nodesTag) {
					if (n.Attributes["nicename"] != null) {
						wpp.Tags.Add(n.Attributes["nicename"].Value);
					}
				}

				wpp.CleanBody();

				lstWPP.Add(wpp);

				//=================
				XmlNodeList nodesC = node.SelectNodes("wp:comment", rssNamespace);
				foreach (XmlNode nodeC in nodesC) {
					WordPressComment wpc = new WordPressComment();
					wpc.ImportRootID = Guid.Empty;
					wpc.PostID = wpp.PostID;
					wpc.CommentID = int.Parse(nodeC.SelectSingleNode("wp:comment_id", rssNamespace).InnerText);
					wpc.Author = nodeC.SelectSingleNode("wp:comment_author", rssNamespace).InnerText;
					wpc.AuthorIP = nodeC.SelectSingleNode("wp:comment_author_IP", rssNamespace).InnerText;
					wpc.AuthorEmail = nodeC.SelectSingleNode("wp:comment_author_email", rssNamespace).InnerText;
					wpc.AuthorURL = nodeC.SelectSingleNode("wp:comment_author_url", rssNamespace).InnerText;
					wpc.CommentContent = nodeC.SelectSingleNode("wp:comment_content", rssNamespace).InnerText;

					wpc.Approved = nodeC.SelectSingleNode("wp:comment_approved", rssNamespace).InnerText;
					wpc.Type = nodeC.SelectSingleNode("wp:comment_type", rssNamespace).InnerText;

					wpc.CommentDateUTC = DateTime.UtcNow;
					try { wpc.CommentDateUTC = Convert.ToDateTime(nodeC.SelectSingleNode("wp:comment_date", rssNamespace).InnerText); } catch { }
					try { wpc.CommentDateUTC = Convert.ToDateTime(nodeC.SelectSingleNode("wp:comment_date_gmt", rssNamespace).InnerText); } catch { }

					lstWPC.Add(wpc);
				}
			}

			foreach (WordPressPost w in lstWPP.Where(x => x.ParentPostID > 0 && x.PostType == WordPressPost.WPPostType.Page)) {
				if (lstWPP.Where(x => x.PostID == w.ParentPostID
							&& x.PostType == WordPressPost.WPPostType.Page).Count() > 0) {
					WordPressPost p = lstWPP.Where(x => x.PostID == w.ParentPostID).FirstOrDefault();
					w.ImportFileName = "/" + p.PostName.Trim() + w.ImportFileSlug;
				}
			}

			site.Content = lstWPP;
			site.Comments = lstWPC;
			site.Authors = lstAuth;

			return site;
		}

		public WordPressSite GetContent(XmlDocument doc) {
			WordPressSite site = GetAllData(doc);

			site.Content.RemoveAll(x => x.PostType == WordPressPost.WPPostType.Attachment || x.PostType == WordPressPost.WPPostType.Unknown);

			return site;
		}
	}
}