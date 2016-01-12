using Carrotware.CMS.Data;
using Carrotware.CMS.Interface;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;

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

	public class TextWidget {

		public TextWidget() { }

		public Guid TextWidgetID { get; set; }
		public Guid SiteID { get; set; }
		public string TextWidgetAssembly { get; set; }

		public bool ProcessBody { get; set; }
		public bool ProcessPlainText { get; set; }
		public bool ProcessHTMLText { get; set; }

		public bool ProcessComment { get; set; }
		public bool ProcessSnippet { get; set; }

		private ITextBodyUpdate _txt = null;

		public ITextBodyUpdate TextProcessor {
			get {
				if (_txt == null && !String.IsNullOrEmpty(this.TextWidgetAssembly)) {
					Type t = ReflectionUtilities.GetTypeFromString(this.TextWidgetAssembly);
					Object o = null;

					try {
						o = Activator.CreateInstance(t);
					} catch (Exception ex) {
						o = null;
						SiteData.WriteDebugException("textprocessor", ex);
					}

					if (o != null && o is ITextBodyUpdate) {
						_txt = o as ITextBodyUpdate;
					}
				}

				return _txt;
			}
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is TextWidget) {
				TextWidget p = (TextWidget)obj;
				return (this.SiteID == p.SiteID
						&& this.TextWidgetAssembly.ToLowerInvariant() == p.TextWidgetAssembly.ToLowerInvariant());
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.TextWidgetAssembly.GetHashCode() ^ this.SiteID.GetHashCode();
		}

		internal TextWidget(carrot_TextWidget c) {
			if (c != null) {
				this.TextWidgetID = c.TextWidgetID;
				this.SiteID = c.SiteID;
				this.TextWidgetAssembly = c.TextWidgetAssembly;
				this.ProcessBody = c.ProcessBody;
				this.ProcessPlainText = c.ProcessPlainText;
				this.ProcessHTMLText = c.ProcessHTMLText;
				this.ProcessComment = c.ProcessComment;
				this.ProcessSnippet = c.ProcessSnippet;
			}
		}

		public void Save() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_TextWidget s = CompiledQueries.cqTextWidgetByID(_db, this.TextWidgetID);

				if (s == null) {
					s = new carrot_TextWidget();
					s.TextWidgetID = Guid.NewGuid();
					s.SiteID = this.SiteID;
					s.TextWidgetAssembly = this.TextWidgetAssembly;
					_db.carrot_TextWidgets.InsertOnSubmit(s);
				}

				s.ProcessBody = this.ProcessBody;
				s.ProcessPlainText = this.ProcessPlainText;
				s.ProcessHTMLText = this.ProcessHTMLText;
				s.ProcessComment = this.ProcessComment;
				s.ProcessSnippet = this.ProcessSnippet;

				_db.SubmitChanges();

				this.TextWidgetID = s.TextWidgetID;
			}
		}

		public void Delete() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_TextWidget s = CompiledQueries.cqTextWidgetByID(_db, this.TextWidgetID);

				if (s != null) {
					_db.carrot_TextWidgets.DeleteOnSubmit(s);
					_db.SubmitChanges();
				}
			}
		}

		public static TextWidget Get(Guid textWidgetID) {
			TextWidget _item = null;
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_TextWidget query = CompiledQueries.cqTextWidgetByID(_db, textWidgetID);
				if (query != null) {
					_item = new TextWidget(query);
				}
			}

			return _item;
		}

		public static List<TextWidget> GetSiteTextWidgets(Guid siteID) {
			List<TextWidget> _lst = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_TextWidget> query = CompiledQueries.cqTextWidgetBySiteID(_db, siteID);

				_lst = (from d in query.ToList()
						select new TextWidget(d)).ToList();
			}

			return _lst;
		}
	}
}