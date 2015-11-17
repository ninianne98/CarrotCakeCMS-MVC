using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

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

	public class Widget : IDisposable {
		private CarrotCMSDataContext db = CarrotCMSDataContext.Create();
		//private CarrotCMSDataContext db = CompiledQueries.dbConn;

		public Widget() { }

		public Widget(Guid rootWidgetID) {
			vw_carrot_Widget item = CompiledQueries.cqGetLatestWidget(db, rootWidgetID);

			SetVals(item);
		}

		public void LoadPageWidgetVersion(Guid widgetDataID) {
			vw_carrot_Widget item = CompiledQueries.cqGetWidgetDataByID_VW(db, widgetDataID);

			SetVals(item);
		}

		public Widget(vw_carrot_Widget w) {
			SetVals(w);
		}

		private void SetVals(vw_carrot_Widget ww) {
			if (ww != null) {
				SiteData site = SiteData.GetSiteFromCache(ww.SiteID);

				this.IsWidgetPendingDelete = false;
				this.IsPendingChange = false;
				this.WidgetDataID = ww.WidgetDataID;

				this.EditDate = site.ConvertUTCToSiteTime(ww.EditDate);
				this.GoLiveDate = site.ConvertUTCToSiteTime(ww.GoLiveDate);
				this.RetireDate = site.ConvertUTCToSiteTime(ww.RetireDate);

				this.IsLatestVersion = ww.IsLatestVersion;
				this.ControlProperties = ww.ControlProperties;

				this.Root_WidgetID = ww.Root_WidgetID;
				this.Root_ContentID = ww.Root_ContentID;
				this.WidgetOrder = ww.WidgetOrder;
				this.ControlPath = ww.ControlPath;
				this.PlaceholderName = ww.PlaceholderName;
				this.IsWidgetActive = ww.WidgetActive;
			}
		}

		[Display(Name = "Control Path")]
		public string ControlPath { get; set; }

		[Display(Name = "Control Properties")]
		public string ControlProperties { get; set; }

		public Guid WidgetDataID { get; set; }
		public Guid Root_WidgetID { get; set; }

		[Display(Name = "PlaceholderName")]
		public string PlaceholderName { get; set; }

		public Guid Root_ContentID { get; set; }
		public int WidgetOrder { get; set; }

		[Display(Name = "Latest Version")]
		public bool IsLatestVersion { get; set; }

		[Display(Name = "Active")]
		public bool IsWidgetActive { get; set; }

		[Display(Name = "Pending Delete")]
		public bool IsWidgetPendingDelete { get; set; }

		[Display(Name = "Selected Item")]
		public bool Selected { get; set; }

		[Display(Name = "Pending Change")]
		public bool IsPendingChange { get; set; }

		[Display(Name = "Edit Date")]
		public DateTime EditDate { get; set; }

		[Display(Name = "Go Live Date")]
		public DateTime GoLiveDate { get; set; }

		[Display(Name = "Retire Date")]
		public DateTime RetireDate { get; set; }

		[Display(Name = "Retired")]
		public bool IsRetired {
			get {
				if (this.RetireDate < SiteData.CurrentSite.Now) {
					return true;
				} else {
					return false;
				}
			}
		}

		[Display(Name = "Un Released")]
		public bool IsUnReleased {
			get {
				if (this.GoLiveDate > SiteData.CurrentSite.Now) {
					return true;
				} else {
					return false;
				}
			}
		}

		public void Save() {
			if (!this.IsWidgetPendingDelete) {
				SiteData site = new SiteData(CompiledQueries.cqGetSiteFromRootContentID(db, this.Root_ContentID));

				carrot_Widget w = CompiledQueries.cqGetRootWidget(db, this.Root_WidgetID);

				bool bAdd = false;
				if (w == null) {
					bAdd = true;
					w = new carrot_Widget();
				}

				if (this.Root_WidgetID == Guid.Empty) {
					this.Root_WidgetID = Guid.NewGuid();
				}

				if (this.GoLiveDate.Year < 1900) {
					this.GoLiveDate = site.Now.AddMinutes(-5);
				}
				if (this.RetireDate.Year < 1900) {
					this.RetireDate = site.Now.AddYears(200);
				}

				w.Root_WidgetID = this.Root_WidgetID;

				w.WidgetOrder = this.WidgetOrder;
				w.Root_ContentID = this.Root_ContentID;
				w.PlaceholderName = this.PlaceholderName;
				w.ControlPath = this.ControlPath.Replace("~~/", "~/").Replace(@"//", @"/");
				w.WidgetActive = this.IsWidgetActive;
				w.GoLiveDate = site.ConvertSiteTimeToUTC(this.GoLiveDate);
				w.RetireDate = site.ConvertSiteTimeToUTC(this.RetireDate);

				carrot_WidgetData wd = new carrot_WidgetData();
				wd.Root_WidgetID = w.Root_WidgetID;
				wd.WidgetDataID = Guid.NewGuid();
				wd.IsLatestVersion = true;
				wd.ControlProperties = this.ControlProperties;
				wd.EditDate = DateTime.UtcNow;

				carrot_WidgetData oldWD = CompiledQueries.cqGetWidgetDataByRootID(db, this.Root_WidgetID);

				//only add a new entry if the widget has some sort of change in the data stored.
				if (oldWD != null) {
					if (oldWD.ControlProperties != wd.ControlProperties) {
						oldWD.IsLatestVersion = false;
						db.carrot_WidgetDatas.InsertOnSubmit(wd);
					}
				} else {
					db.carrot_WidgetDatas.InsertOnSubmit(wd);
				}

				if (bAdd) {
					db.carrot_Widgets.InsertOnSubmit(w);
				}

				db.SubmitChanges();
			} else {
				DeleteAll();
			}
		}

		public void DeleteAll() {
			IQueryable<carrot_WidgetData> w1 = CannedQueries.GetWidgetDataByRootAll(db, this.Root_WidgetID);

			carrot_Widget w2 = CompiledQueries.cqGetRootWidget(db, this.Root_WidgetID);

			bool bPendingDel = false;

			if (w1 != null) {
				db.carrot_WidgetDatas.BatchDelete(w1);
				bPendingDel = true;
			}

			if (w2 != null) {
				db.carrot_Widgets.DeleteOnSubmit(w2);
				bPendingDel = true;
			}

			if (bPendingDel) {
				db.SubmitChanges();
			}
		}

		public void Disable() {
			carrot_Widget w = CompiledQueries.cqGetRootWidget(db, this.Root_WidgetID);

			if (w != null) {
				w.WidgetActive = false;
				db.SubmitChanges();
			}
		}

		public List<WidgetProps> ParseDefaultControlProperties() {
			List<WidgetProps> props = new List<WidgetProps>();
			string sProps = this.ControlProperties;

			if (!String.IsNullOrEmpty(sProps) && sProps.StartsWith("<?xml version=\"1.0\"")
					&& sProps.Contains("<KeyName") && sProps.Contains("<KeyValue")) {
				if (sProps.Contains("<ArrayOfWidgetProps")) {
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<WidgetProps>));
					Object genpref = null;
					using (StringReader stringReader = new StringReader(sProps)) {
						genpref = xmlSerializer.Deserialize(stringReader);
					}
					props = genpref as List<WidgetProps>;
				}
				if (sProps.Contains("<DefaultControlProperties")) {
					props = ParseDefaultControlPropertiesOld(sProps);
				}
			}

			return props;
		}

		public void SaveDefaultControlProperties(List<WidgetProps> props) {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<WidgetProps>));
			string sXML = "";
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, props);
				sXML = stringWriter.ToString();
			}

			this.ControlProperties = sXML;
		}

		private List<WidgetProps> ParseDefaultControlPropertiesOld(string sProps) {
			List<WidgetProps> props = new List<WidgetProps>();

			if (!String.IsNullOrEmpty(sProps) && sProps.StartsWith("<?xml")) {
				DataSet ds = new DataSet();
				using (StringReader stream = new StringReader(sProps)) {
					ds.ReadXml(stream);
				}

				props = (from d in ds.Tables[0].AsEnumerable()
						 select new WidgetProps {
							 KeyName = d.Field<string>("KeyName"),
							 KeyValue = d.Field<string>("KeyValue")
						 }).ToList();
			}

			return props;
		}

		private void SaveDefaultControlPropertiesOld(List<WidgetProps> props) {
			DataSet ds = new DataSet("DefaultControlProperties");
			DataTable dt = new DataTable("ControlProperties");
			DataColumn dc1 = new DataColumn("KeyName", typeof(System.String));
			DataColumn dc2 = new DataColumn("KeyValue", typeof(System.String));
			dt.Columns.Add(dc1);
			dt.Columns.Add(dc2);
			ds.Tables.Add(dt);

			foreach (WidgetProps p in props) {
				DataRow newRow = ds.Tables["ControlProperties"].NewRow();
				newRow["KeyName"] = p.KeyName;
				newRow["KeyValue"] = p.KeyValue;
				ds.Tables["ControlProperties"].Rows.Add(newRow);
			}

			ds.AcceptChanges();

			this.ControlProperties = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>     " + ds.GetXml();
		}

		#region IDisposable Members

		public void Dispose() {
			if (db != null) {
				db.Dispose();
			}
		}

		#endregion IDisposable Members
	}

	public class WidgetProps {

		public WidgetProps() { }

		public string KeyName { get; set; }
		public string KeyValue { get; set; }
	}
}