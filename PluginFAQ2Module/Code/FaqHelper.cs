using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CarrotCake.CMS.Plugins.FAQ2 {

	public class FaqHelper : IDisposable {
		protected FAQDataContext db = FAQDataContext.GetDataContext();

		public FaqHelper() { }

		public FaqHelper(Guid siteID) {
			this.SiteID = siteID;
		}

		public Guid SiteID { get; set; }

		public carrot_FaqItem FaqItemGetByID(Guid faqItemID) {
			carrot_FaqItem ff = (from f in db.carrot_FaqItems
								 where f.FaqItemID == faqItemID
								 select f).FirstOrDefault();

			return ff;
		}

		public List<carrot_FaqItem> FaqItemListGetByFaqCategoryID(Guid faqCategoryID) {
			List<carrot_FaqItem> ff = (from f in db.carrot_FaqItems
									   where f.FaqCategoryID == faqCategoryID
									   orderby f.ItemOrder
									   select f).ToList();

			return ff;
		}

		public List<carrot_FaqItem> FaqItemListPublicGetByFaqCategoryID(Guid faqCategoryID, Guid siteID) {
			List<carrot_FaqItem> ff = (from f in db.carrot_FaqItems
									   join fc in db.carrot_FaqCategories on f.FaqCategoryID equals fc.FaqCategoryID
									   where f.FaqCategoryID == faqCategoryID
												&& f.IsActive == true && fc.SiteID == siteID
									   orderby f.ItemOrder
									   select f).ToList();

			return ff;
		}

		public List<carrot_FaqItem> FaqItemListPublicTopGetByFaqCategoryID(Guid faqCategoryID, Guid siteID, int takeCount) {
			if (takeCount < 0) {
				takeCount = 1;
			}

			if (takeCount > 100) {
				takeCount = 100;
			}

			List<carrot_FaqItem> ff = (from f in db.carrot_FaqItems
									   join fc in db.carrot_FaqCategories on f.FaqCategoryID equals fc.FaqCategoryID
									   where f.FaqCategoryID == faqCategoryID
												&& f.IsActive == true && fc.SiteID == siteID
									   orderby f.ItemOrder
									   select f).Take(takeCount).ToList();

			return ff;
		}

		public carrot_FaqItem FaqItemListPublicRandGetByFaqCategoryID(Guid faqCategoryID, Guid siteID) {
			Random rand = new Random();

			int toSkip = rand.Next(0, (from f in db.carrot_FaqItems
									   join fc in db.carrot_FaqCategories on f.FaqCategoryID equals fc.FaqCategoryID
									   where f.FaqCategoryID == faqCategoryID
												&& f.IsActive == true && fc.SiteID == siteID
									   orderby f.ItemOrder
									   select f).Count());

			carrot_FaqItem ff = (from f in db.carrot_FaqItems
								 join fc in db.carrot_FaqCategories on f.FaqCategoryID equals fc.FaqCategoryID
								 where f.FaqCategoryID == faqCategoryID
										  && f.IsActive == true && fc.SiteID == siteID
								 orderby f.ItemOrder
								 select f).Skip(toSkip).FirstOrDefault();

			return ff;
		}

		public void FAQImageCleanup(Guid faqCategoryID, List<Guid> lst) {
			var lstDel = (from f in db.carrot_FaqItems
						  where f.FaqCategoryID == faqCategoryID
								&& !lst.Contains(f.FaqItemID)
						  select f).ToList();

			db.carrot_FaqItems.DeleteAllOnSubmit(lstDel);

			db.SubmitChanges();
		}

		public carrot_FaqCategory CategoryGetByID(Guid faqCategoryID) {
			carrot_FaqCategory ge = (from c in db.carrot_FaqCategories
									 where c.SiteID == this.SiteID
											&& c.FaqCategoryID == faqCategoryID
									 select c).FirstOrDefault();

			return ge;
		}

		public List<carrot_FaqCategory> CategoryListGetBySiteID() {
			return CategoryListGetBySiteID(this.SiteID);
		}

		public List<carrot_FaqCategory> CategoryListGetBySiteID(Guid siteID) {
			List<carrot_FaqCategory> ge = (from c in db.carrot_FaqCategories
										   orderby c.FAQTitle
										   where c.SiteID == siteID
										   select c).ToList();

			return ge;
		}

		public carrot_FaqCategory Save(carrot_FaqCategory item) {
			if (item.FaqCategoryID == Guid.Empty) {
				item.FaqCategoryID = Guid.NewGuid();
			}

			if (!db.carrot_FaqCategories.Where(x => x.FaqCategoryID == item.FaqCategoryID).Any()) {
				db.carrot_FaqCategories.InsertOnSubmit(item);
			}

			db.SubmitChanges();

			return item;
		}

		public carrot_FaqItem Save(carrot_FaqItem item) {
			if (item.FaqItemID == Guid.Empty) {
				item.FaqItemID = Guid.NewGuid();
			}

			if (!db.carrot_FaqItems.Where(x => x.FaqItemID == item.FaqItemID).Any()) {
				db.carrot_FaqItems.InsertOnSubmit(item);
			}

			db.SubmitChanges();

			return item;
		}

		public bool DeleteItem(Guid ItemGuid) {
			var itm = (from c in db.carrot_FaqItems
					   where c.FaqItemID == ItemGuid
					   select c).FirstOrDefault();

			db.carrot_FaqItems.DeleteOnSubmit(itm);

			db.SubmitChanges();

			return true;
		}

		public static string ReadEmbededScript(string sResouceName) {
			string sReturn = null;

			Assembly _assembly = Assembly.GetExecutingAssembly();
			using (var stream = new StreamReader(_assembly.GetManifestResourceStream(sResouceName))) {
				sReturn = stream.ReadToEnd();
			}

			return sReturn;
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