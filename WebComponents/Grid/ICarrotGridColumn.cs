using System;
using System.Web;

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

	public interface ICarrotGridColumn {
		string HeaderText { get; set; }
		object HeadAttributes { get; set; }
		object BodyAttributes { get; set; }
		bool HasHeadingText { get; set; }
		int Order { get; set; }
		CarrotGridColumnType Mode { get; set; }
	}

	public interface ICarrotGridColumnTemplate<T> where T : class {
		Func<T, IHtmlString> FormatTemplate { get; set; }
	}

	public interface ICarrotGridColumnExt {
		string ColumnName { get; set; }
		bool Sortable { get; set; }
		object HeadLinkAttributes { get; set; }
		string CellFormatString { get; set; }
	}
}