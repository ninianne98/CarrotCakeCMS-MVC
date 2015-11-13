using System;
using System.Collections.Generic;
using System.Data;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public class PagedDataTable : PagedDataBase {

		public PagedDataTable()
			: base() {
		}

		public void InitOrderBy(string columnName) {
			InitOrderBy(columnName, true);
		}

		public void InitOrderBy(string columnName, bool ascending) {
			if (ascending) {
				this.OrderBy = String.Format("{0}  ASC", columnName);
			} else {
				this.OrderBy = String.Format("{0}  DESC", columnName);
			}
		}

		public void SetData(DataTable data) {
			this.DataSource = data;
		}

		public void SetData(DataSet data) {
			SetData(data, 0);
		}

		public void SetData(DataSet data, string tableName) {
			SetData(data.Tables[tableName]);
		}

		public void SetData(DataSet data, int tableIndex) {
			SetData(data.Tables[tableIndex]);
		}

		public DataTable DataSource { get; set; }

		public override bool HasData {
			get {
				return this.DataSource != null && this.DataSource.Rows != null
					&& this.DataSource.Rows.Count > 0;
			}
		}
	}
}