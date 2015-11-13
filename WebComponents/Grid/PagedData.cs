using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

	public class PagedData<T> : PagedDataBase {

		public PagedData()
			: base() {
		}

		public void InitOrderBy(Expression<Func<T, object>> field) {
			InitOrderBy(field, true);
		}

		public void InitOrderBy(Expression<Func<T, object>> field, bool ascending) {
			MemberExpression memberExpression = field.Body as MemberExpression ??
												((UnaryExpression)field.Body).Operand as MemberExpression;

			string columnName = memberExpression.Member.Name;

			if (ascending) {
				this.OrderBy = String.Format("{0}  ASC", columnName);
			} else {
				this.OrderBy = String.Format("{0}  DESC", columnName);
			}
		}

		public void SetData(List<T> data) {
			this.DataSource = data;
		}

		public List<T> DataSource { get; set; }

		public override bool HasData {
			get {
				return this.DataSource != null && this.DataSource.Any();
			}
		}
	}
}