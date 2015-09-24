using Carrotware.CMS.Interface.Controllers;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Northwind.Controllers {
	public class AdminController : BaseAdminWidgetController {

		protected NorthwindDataContext db = new NorthwindDataContext();

		// GET: Admin
		public ActionResult Products() {

			PagedData<Product> model = new PagedData<Product>();
			model.InitOrderBy(x => x.ProductName);
			model.PageSize = 20;

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<Product>(db.Products, srt.SortField, srt.SortDirection);

			model.DataSource = query.Take(model.PageSize).ToList();
			model.TotalRecords = db.Products.Count();
			ViewBag.SupplierList = db.Suppliers.ToList();

			return View(model);
		}

		[HttpPost]
		public ActionResult Products(PagedData<Product> model) {

			model.ToggleSort();
			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<Product>(db.Products, srt.SortField, srt.SortDirection);

			model.DataSource = query.Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();
			model.TotalRecords = db.Products.Count();
			ViewBag.SupplierList = db.Suppliers.ToList();

			ModelState.Clear();

			return View(model);
		}


		public ActionResult Suppliers() {

			PagedData<Supplier> model = new PagedData<Supplier>();
			model.InitOrderBy(x => x.CompanyName);
			model.PageSize = 10;

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<Supplier>(db.Suppliers, srt.SortField, srt.SortDirection);

			model.DataSource = query.Take(model.PageSize).ToList();
			model.TotalRecords = db.Suppliers.Count();

			return View(model);
		}

		[HttpPost]
		public ActionResult Suppliers(PagedData<Supplier> model) {

			model.ToggleSort();
			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<Supplier>(db.Suppliers, srt.SortField, srt.SortDirection);

			model.DataSource = query.Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();
			model.TotalRecords = db.Suppliers.Count();

			ModelState.Clear();

			return View(model);
		}

		public ActionResult ViewSupplier(int id) {

			return View(db.Suppliers.Where(x => x.SupplierID == id).FirstOrDefault());
		}


		protected void LoadDDLs() {
			ViewBag.SupplierList = db.Suppliers.ToList();
			ViewBag.CategoryList = db.Categories.ToList();
		}

		public ActionResult EditProduct(int id) {
			LoadDDLs();

			return View(db.Products.Where(x => x.ProductID == id).FirstOrDefault());
		}

		public ActionResult CreateProduct() {
			LoadDDLs();
			return View("EditProduct", new Product());
		}

		[HttpPost]
		public ActionResult EditProduct(Product model) {

			return CreateProduct(model);
		}

		[HttpPost]
		public ActionResult CreateProduct(Product model) {
			LoadDDLs();

			Product prod = db.Products.Where(x => x.ProductID == model.ProductID).FirstOrDefault();

			if (prod == null) {
				prod = model;
				db.Products.InsertOnSubmit(prod);
			} else {
				prod.ProductName = model.ProductName;
				prod.UnitPrice = model.UnitPrice;
				prod.UnitsInStock = model.UnitsInStock;
				prod.UnitsOnOrder = model.UnitsOnOrder;
				prod.ReorderLevel = model.ReorderLevel;
				prod.Discontinued = model.Discontinued;
			}

			db.SubmitChanges();

			return View("EditProduct", new Product());
		}


		public ActionResult Employees() {
			PagedData<Employee> model = new PagedData<Employee>();
			model = InitEmpData();

			return View(model);
		}

		[HttpPost]
		public ActionResult Employees(PagedData<Employee> model) {
			model = GetEmpData(model);

			ModelState.Clear();

			return View(model);
		}

		private PagedData<Employee> InitEmpData() {
			PagedData<Employee> model = new PagedData<Employee>();
			model.InitOrderBy(x => x.LastName, true);
			model.PageSize = 5;

			model.DataSource = (from c in db.Employees
								orderby c.LastName ascending
								select c).Take(model.PageSize).ToList();

			model.TotalRecords = (from c in db.Employees
								  select c).Count();

			ViewBag.TerritoryList = db.Territories.ToList();

			return model;
		}

		private PagedData<Employee> GetEmpData(PagedData<Employee> model) {
			List<Employee> lst = new List<Employee>();
			model.PageSize = 5;

			model.ToggleSort();
			var srt = model.ParseSort();

			model.DataSource = new List<Employee>();
			IQueryable<Employee> query = (from c in db.Employees select c);

			query = query.SortByParm<Employee>(srt.SortField, srt.SortDirection);

			model.DataSource = query.Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

			model.TotalRecords = (from c in db.Employees select c).Count();

			ViewBag.TerritoryList = db.Territories.ToList();

			model.SortByNew = String.Empty;

			return model;
		}


		public ActionResult ViewEmployee(int id) {
			var model = (from c in db.Employees
						 where c.EmployeeID == id
						 select c).FirstOrDefault();

			return View(model);
		}




		protected override void Dispose(bool disposing) {
			if (db != null) {
				db.Dispose();
			}

			base.Dispose(disposing);
		}


	}
}