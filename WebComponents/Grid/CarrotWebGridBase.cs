using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

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

	public abstract class CarrotWebGridBase : IHtmlString {
		protected HtmlHelper _htmlHelper;
		protected SortParm _sortDir;

		protected void StandardInit(HtmlHelper htmlHelper, PagedDataBase dp) {
			_htmlHelper = htmlHelper;

			this.FooterOuterTag = "ul";
			this.FooterTag = "li";

			this.FieldIdPrefix = string.Empty;
			this.FieldNamePrefix = string.Empty;

			this.SortDescIndicator = "&nbsp;&#9660;";
			this.SortAscIndicator = "&nbsp;&#9650;";

			this.HtmlClientId = "tblDataTable";

			this.Columns = new List<ICarrotGridColumn>();

			this.UseDataPage = true;
			this.PageSizeExternal = false;

			this.PagedDataBase = dp;
		}

		protected PagedDataBase PagedDataBase { get; set; }

		public List<ICarrotGridColumn> Columns { get; protected set; }

		public Func<Object, IHtmlString> EmptyDataTemplate { get; set; }

		public string HtmlClientId { get; set; }
		public string HtmlFormId { get; set; }
		public string SortDescIndicator { get; set; }
		public string SortAscIndicator { get; set; }
		protected string FieldIdPrefix { get; set; }
		protected string FieldNamePrefix { get; set; }

		public int RowNumber { get; set; }

		public bool UseDataPage { get; set; }
		public bool PageSizeExternal { get; set; }

		public string FooterOuterTag { get; set; }
		public object htmlFootAttrib { get; set; }

		public string FooterTag { get; set; }
		public object htmlFootSel { get; set; }
		public object htmlFootNotSel { get; set; }

		public void ConfigName(IHtmlString name) {
			this.FieldNamePrefix = name.ToString();

			if (string.IsNullOrEmpty(this.FieldNamePrefix)) {
				this.FieldNamePrefix = string.Empty;
			} else {
				this.FieldNamePrefix = string.Format("{0}.", this.FieldNamePrefix);
			}

			this.FieldIdPrefix = this.FieldNamePrefix.Replace(".", "_").Replace("]", "_").Replace("[", "_");
		}

		public object TableAttributes { get; set; }
		public object THeadAttributes { get; set; }
		public object TBodyAttributes { get; set; }

		protected IDictionary<string, object> InitAttrib(object htmlAttribs) {
			IDictionary<string, object> tblAttrib = (IDictionary<string, object>)new RouteValueDictionary();

			if (htmlAttribs != null) {
				tblAttrib = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttribs);
			}

			return tblAttrib;
		}

		protected void FormHelper(Expression<Func<PagedDataBase, object>> property, StringBuilder sb) {
			PropertyInfo propInfo = this.PagedDataBase.PropInfoFromExpression<PagedDataBase>(property);
			string columnName = ReflectionUtilities.BuildProp(property);

			object val = propInfo.GetValue(this.PagedDataBase, null);

			string fldName = string.Format("{0}{1}", this.FieldNamePrefix, columnName);
			string str = val == null ? string.Empty : val.ToString();

			sb.AppendLine(_htmlHelper.Hidden(fldName, str).ToString());
		}

		protected StringBuilder BuildHeadScript(StringBuilder sb) {
			string frm = "form:first";
			if (!string.IsNullOrEmpty(this.HtmlFormId)) {
				frm = string.Format("#{0}", this.HtmlFormId);
			}

			if (this.UseDataPage) {
				sb.AppendLine(string.Empty);
				sb.AppendLine("	<script type=\"text/javascript\">");
				sb.AppendLine("	function __clickHead(fld) {");
				sb.AppendLine(string.Format("		$('#{0}SortByNew').val(fld);", this.FieldIdPrefix));
				sb.AppendLine(string.Format("		$('{0}')[0].submit();", frm));
				sb.AppendLine("	}");
				sb.AppendLine(string.Empty);
				sb.AppendLine("	function __clickPage(nbr, fld) {");
				sb.AppendLine("		$('#' + fld).val(nbr);");
				sb.AppendLine("		$('#' + fld).focus();");
				sb.AppendLine(string.Format("		$('{0}')[0].submit();", frm));
				sb.AppendLine("	}");
				sb.AppendLine("	</script>");
				sb.AppendLine(string.Empty);

				FormHelper(x => x.OrderBy, sb);
				FormHelper(x => x.SortByNew, sb);
				if (!this.PageSizeExternal) {
					FormHelper(x => x.PageSize, sb);
				}
				FormHelper(x => x.TotalRecords, sb);
				FormHelper(x => x.MaxPage, sb);
				FormHelper(x => x.PageNumber, sb);
			}

			return sb;
		}

		protected StringBuilder BuildTableHeadRow(StringBuilder sb) {
			using (new WrappedItem(sb, "thead", this.THeadAttributes)) {
				using (new WrappedItem(sb, "tr", null)) {
					foreach (var col in this.Columns) {
						using (new WrappedItem(sb, "th", col.HeadAttributes)) {
							if (col is ICarrotGridColumnExt) {
								var colExt = (ICarrotGridColumnExt)col;
								if (colExt.Sortable && this.UseDataPage) {
									string js = string.Format("javascript:__clickHead('{0}')", colExt.ColumnName);

									IDictionary<string, object> tagAttrib = InitAttrib(colExt.HeadLinkAttributes);

									tagAttrib.Add("href", js);

									using (new WrappedItem(sb, "a", tagAttrib)) {
										sb.Append(col.HeaderText);

										if (_sortDir.SortField.ToUpperInvariant() == colExt.ColumnName.ToUpperInvariant()) {
											if (_sortDir.SortDirection.ToUpperInvariant() == "ASC") {
												sb.Append(this.SortAscIndicator);
											} else {
												sb.Append(this.SortDescIndicator);
											}
										}
									}
								} else {
									sb.Append(col.HeaderText);
								}
							} else {
								sb.Append(col.HeaderText);
							}
						}
					}
				}
			}

			return sb;
		}

		protected virtual IHtmlString CreateBody() {
			return new HtmlString(string.Empty);
		}

		public virtual void SetupFooter(string outer, object outerAttrib, string inner, object selAttrib, object noselAttrib) {
			this.FooterOuterTag = string.IsNullOrEmpty(outer) ? "ul" : outer;

			this.htmlFootAttrib = outerAttrib;

			this.FooterTag = string.IsNullOrEmpty(inner) ? "li" : inner;

			this.htmlFootSel = selAttrib;
			this.htmlFootNotSel = noselAttrib;
		}

		public virtual IHtmlString OutputFooter() {
			var sb = new StringBuilder();

			if (this.PagedDataBase.TotalPages > 1) {
				using (new WrappedItem(sb, this.FooterOuterTag, this.htmlFootAttrib)) {
					foreach (var i in this.PagedDataBase.PageNumbers) {
						string clickFn = string.Format("javascript:__clickPage('{0}','{1}PageNumber')", i, this.FieldIdPrefix);

						using (new WrappedItem(_htmlHelper, sb, this.FooterTag, i, this.PagedDataBase.PageNumber, this.htmlFootSel, this.htmlFootNotSel)) {
							using (new WrappedItem(sb, "a", new { @href = clickFn })) {
								sb.Append(string.Format(" {0} ", i));
							}
						}
					}
				}
			}

			return new HtmlString(sb.ToString());
		}

		protected virtual IHtmlString EmptyTable() {
			this.PagedDataBase.TotalRecords = 0;
			this.PagedDataBase.PageNumber = 1;

			string cellContents = string.Empty;

			var sb = new StringBuilder();

			FormHelper(x => x.OrderBy, sb);
			FormHelper(x => x.SortByNew, sb);
			if (!this.PageSizeExternal) {
				FormHelper(x => x.PageSize, sb);
			}
			FormHelper(x => x.TotalRecords, sb);
			FormHelper(x => x.MaxPage, sb);
			FormHelper(x => x.PageNumber, sb);

			if ((!this.PagedDataBase.HasData) && this.EmptyDataTemplate != null) {
				cellContents = this.EmptyDataTemplate(new object()).ToHtmlString();
			}

			sb.AppendLine(cellContents);

			return new HtmlString(sb.ToString());
		}

		public virtual IHtmlString OutputHtmlBody() {
			if (this.PagedDataBase.HasData) {
				return CreateBody();
			} else {
				return EmptyTable();
			}
		}

		public string ToHtmlString() {
			var sb = new StringBuilder();
			sb.AppendLine(this.OutputHtmlBody().ToString());
			sb.AppendLine(this.OutputFooter().ToString());
			return sb.ToString();
		}

		public IHtmlString Write() {
			return new HtmlString(ToHtmlString());
		}
	}
}