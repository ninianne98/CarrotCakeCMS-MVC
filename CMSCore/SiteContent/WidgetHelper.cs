using Carrotware.CMS.Data;
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

	public class WidgetHelper : IDisposable {
		private CarrotCMSDataContext db = CarrotCMSDataContext.Create();
		//private CarrotCMSDataContext db = CompiledQueries.dbConn;

		public WidgetHelper() { }

		public Widget Get(Guid rootWidgetID) {
			return new Widget(rootWidgetID);
		}

		public List<Widget> GetWidgets(Guid rootContentID, bool bActiveOnly) {
			List<Widget> w = (from r in CompiledQueries.cqGetLatestWidgets(db, rootContentID, bActiveOnly)
							  select new Widget(r)).ToList();

			return w;
		}

		public List<Widget> GetWidgetVersionHistory(Guid rootWidgetID) {
			List<Widget> w = (from r in CompiledQueries.cqGetWidgetVersionHistory_VW(db, rootWidgetID)
							  select new Widget(r)).ToList();

			return w;
		}

		public Widget GetWidgetVersion(Guid widgetDataID) {
			Widget w = new Widget(CompiledQueries.cqGetWidgetDataByID_VW(db, widgetDataID));

			return w;
		}

		public void RemoveVersions(List<Guid> lstDel) {
			IQueryable<carrot_WidgetData> oldW = (from w in db.carrot_WidgetDatas
												  orderby w.EditDate descending
												  where lstDel.Contains(w.WidgetDataID)
												  && w.IsLatestVersion != true
												  select w);

			db.carrot_WidgetDatas.DeleteBatch(oldW);
			db.SubmitChanges();
		}

		public void Delete(Guid widgetDataID) {
			carrot_WidgetData w = CompiledQueries.cqGetWidgetDataByID_TBL(db, widgetDataID);

			if (w != null) {
				db.carrot_WidgetDatas.DeleteOnSubmit(w);
				db.SubmitChanges();
			}
		}

		public void Disable(Guid rootWidgetID) {
			carrot_Widget w = CompiledQueries.cqGetRootWidget(db, rootWidgetID);

			if (w != null) {
				w.WidgetActive = false;
				db.SubmitChanges();
			}
		}

		public void SetStatusList(Guid rootContentID, List<Guid> lstWidgetIDs, bool widgetStatus) {
			IQueryable<carrot_Widget> queryWidgets = (from w in CannedQueries.GetWidgetsByRootContent(db, rootContentID)
													  where lstWidgetIDs.Contains(w.Root_WidgetID)
															&& w.WidgetActive != widgetStatus
													  select w);

			db.carrot_Widgets.UpdateBatch(queryWidgets, p => new carrot_Widget { WidgetActive = widgetStatus });

			db.SubmitChanges();
		}

		public void DeleteAll(Guid rootWidgetID) {
			IQueryable<carrot_WidgetData> w1 = CannedQueries.GetWidgetDataByRootAll(db, rootWidgetID);

			carrot_Widget w2 = CompiledQueries.cqGetRootWidget(db, rootWidgetID);

			bool bPendingDel = false;

			if (w1 != null) {
				db.carrot_WidgetDatas.DeleteBatch(w1);
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

		#region IDisposable Members

		public void Dispose() {
			if (db != null) {
				db.Dispose();
			}
		}

		#endregion IDisposable Members
	}
}