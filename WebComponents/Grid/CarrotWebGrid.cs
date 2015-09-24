using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public enum GridFormFieldType {
		Checkbox,
		TextBox,
		RadioButton,
		TextArea,
		Hidden,
	}

	public class CarrotWebGrid<T> : IHtmlString where T : class {
		private HtmlHelper _htmlHelper;
		private SortParm _sortDir;
		public PagedData<T> DataPage { get; set; }

		public List<ICarrotGridColumn> Columns { get; protected set; }

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

		public Func<Object, HelperResult> EmptyDataTemplate { get; set; }

		public CarrotWebGrid(HtmlHelper htmlHelper)
			: this(htmlHelper, new PagedData<T>()) {
			this.UseDataPage = false;
		}

		public CarrotWebGrid(HtmlHelper htmlHelper, PagedData<T> dp) {
			this.DataPage = dp;
			_htmlHelper = htmlHelper;

			this.FooterOuterTag = "ul";
			this.FooterTag = "li";

			this.FieldIdPrefix = String.Empty;
			this.FieldNamePrefix = String.Empty;

			this.SortDescIndicator = "&nbsp;&#9660;";
			this.SortAscIndicator = "&nbsp;&#9650;";

			this.HtmlClientId = "tbl" + typeof(T).Name;

			this.Columns = new List<ICarrotGridColumn>();

			this.UseDataPage = true;
			this.PageSizeExternal = false;
		}

		public CarrotWebGrid<T> AddColumn(Expression<Func<T, Object>> property) {
			AddColumn(property, new CarrotGridColumn());

			return this;
		}

		public CarrotWebGrid<T> AddColumn(Expression<Func<T, Object>> property, ICarrotGridColumn column) {
			MemberExpression memberExpression = property.Body as MemberExpression ??
												((UnaryExpression)property.Body).Operand as MemberExpression;

			if (column.Mode != CarrotGridColumnType.Template) {
				//string columnName = memberExpression.Member.Name;
				string columnName = ReflectionUtilities.BuildProp(property);

				if (column is ICarrotGridColumnExt) {
					var col = (ICarrotGridColumnExt)column;
					col.ColumnName = columnName;
					if (!this.UseDataPage) {
						col.Sortable = false;
					}

					if (String.IsNullOrEmpty(column.HeaderText) && column.HasHeadingText) {
						column.HeaderText = col.ColumnName.Replace(".", " ");

						string displayName = CarrotWeb.DisplayNameFor<T>(property);
						if (!String.IsNullOrEmpty(displayName)) {
							column.HeaderText = displayName;
						}
					}
					if (!column.HasHeadingText && !col.Sortable) {
						column.HeaderText = "  ";
					}
				}
			}

			column.Order = this.Columns.Count();

			this.Columns.Add(column);

			return this;
		}

		public CarrotWebGrid<T> AddColumn(CarrotGridTemplateColumn<T> column) {
			column.Order = this.Columns.Count();

			this.Columns.Add(column);

			return this;
		}

		public void ConfigName(IHtmlString name) {
			this.FieldNamePrefix = name.ToString();

			if (String.IsNullOrEmpty(this.FieldNamePrefix)) {
				this.FieldNamePrefix = String.Empty;
			} else {
				this.FieldNamePrefix = String.Format("{0}.", this.FieldNamePrefix);
			}

			this.FieldIdPrefix = this.FieldNamePrefix.Replace(".", "_").Replace("]", "_").Replace("[", "_");
		}

		public object TableAttributes { get; set; }
		public object THeadAttributes { get; set; }
		public object TBodyAttributes { get; set; }

		private IDictionary<string, object> InitAttrib(object htmlAttribs) {
			IDictionary<string, object> tblAttrib = (IDictionary<string, object>)new RouteValueDictionary();

			if (htmlAttribs != null) {
				tblAttrib = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttribs);
			}

			return tblAttrib;
		}

		private void FormHelper(Expression<Func<PagedData<T>, object>> property, StringBuilder sb) {
			PropertyInfo propInfo = this.DataPage.PropInfoFromExpression<PagedData<T>>(property);
			string columnName = ReflectionUtilities.BuildProp(property);

			Object val = propInfo.GetValue(this.DataPage, null);

			string fldName = String.Format("{0}{1}", this.FieldNamePrefix, columnName);
			string str = val == null ? String.Empty : val.ToString();

			sb.AppendLine(_htmlHelper.Hidden(fldName, str).ToString());
		}

		protected string DataFieldName(string columnName) {
			string fldName = String.Format("{0}DataSource[{1}].{2}", this.FieldNamePrefix, this.RowNumber, columnName);
			if (!this.UseDataPage) {
				if (String.IsNullOrEmpty(this.FieldNamePrefix)) {
					//fldName = fldName.Replace(String.Format("DataSource[{0}]", this.RowNumber), String.Format("[{0}]", this.RowNumber));
					fldName = String.Format("[{0}].{1}", this.RowNumber, columnName);
				} else {
					//fldName = fldName.Replace(String.Format(".DataSource[{0}]", this.RowNumber), String.Format("[{0}]", this.RowNumber));
					fldName = String.Format("{0}[{1}].{2}", this.FieldNamePrefix, this.RowNumber, columnName).Replace(".[", "[");
				}
			}
			return fldName;
		}

		public MvcHtmlString FormFieldFor(Expression<Func<T, Object>> property, GridFormFieldType fldType, object htmlAttribs = null) {
			T row = this.DataPage.DataSource[this.RowNumber];

			//PropertyInfo propInfo = row.PropInfoFromExpression<T>(property);
			//Object val = propInfo.GetValue(row, null);
			string columnName = ReflectionUtilities.BuildProp(property);
			Object val = row.GetPropValueFromExpression(property);

			string fldName = DataFieldName(columnName);

			MvcHtmlString formFld = new MvcHtmlString(String.Empty);

			switch (fldType) {
				case GridFormFieldType.Checkbox:
					if (val == null) {
						formFld = _htmlHelper.CheckBox(fldName, false, htmlAttribs);
					} else {
						formFld = _htmlHelper.CheckBox(fldName, (bool)val, htmlAttribs);
					}
					break;

				case GridFormFieldType.RadioButton:
					formFld = _htmlHelper.RadioButton(fldName, val.ToString(), htmlAttribs);
					break;

				case GridFormFieldType.TextArea:
					formFld = _htmlHelper.TextArea(fldName, val.ToString(), htmlAttribs);
					break;

				case GridFormFieldType.Hidden:
					formFld = _htmlHelper.Hidden(fldName, val.ToString(), htmlAttribs);
					break;

				case GridFormFieldType.TextBox:
				default:
					formFld = _htmlHelper.TextBox(fldName, val.ToString(), htmlAttribs);
					break;
			}

			return formFld;
		}

		public MvcHtmlString DropDownFor(Expression<Func<T, Object>> property, SelectList selectList, string optionLabel, object htmlAttributes = null) {
			T row = this.DataPage.DataSource[this.RowNumber];

			//PropertyInfo propInfo = row.PropInfoFromExpression<T>(property);
			//Object val = propInfo.GetValue(row, null);
			string columnName = ReflectionUtilities.BuildProp(property);
			Object val = row.GetPropValueFromExpression(property);

			string fldName = DataFieldName(columnName);

			MvcHtmlString formFld = new MvcHtmlString(String.Empty);

			if (val != null && selectList.SelectedValue == null) {
				selectList = new SelectList(selectList.Items, selectList.DataValueField, selectList.DataTextField, val);
			}

			if (!String.IsNullOrEmpty(optionLabel)) {
				formFld = _htmlHelper.DropDownList(fldName, selectList, optionLabel, htmlAttributes);
			} else {
				formFld = _htmlHelper.DropDownList(fldName, selectList, htmlAttributes);
			}

			return formFld;
		}

		public MvcHtmlString CheckBoxListFor(Expression<Func<T, Object>> property, MultiSelectList selectList, string selectedFieldName, object chkboxAttributes = null, object listAttributes = null) {
			T row = this.DataPage.DataSource[this.RowNumber];
			string columnName = String.Empty;
			selectedFieldName = String.IsNullOrEmpty(selectedFieldName) ? "Selected" : selectedFieldName;

			if (property.Body.NodeType == ExpressionType.Call) {
				var methodCallExpression = (MethodCallExpression)property.Body;
				columnName = GetInputName(methodCallExpression);
			} else {
				columnName = ReflectionUtilities.BuildProp(property);
			}

			string fldName = DataFieldName(columnName);

			MvcHtmlString formFld = new MvcHtmlString(String.Empty);

			StringBuilder sbChk = new StringBuilder();
			int i = 0;
			using (new WrappedItem(sbChk, "dl", listAttributes)) {
				foreach (var opt in selectList) {
					sbChk.AppendLine("<dt>"
						+ _htmlHelper.Hidden(String.Format("{0}[{1}].{2}", fldName, i, selectList.DataValueField), opt.Value)
						+ _htmlHelper.CheckBox(String.Format("{0}[{1}].{2}", fldName, i, selectedFieldName), opt.Selected, chkboxAttributes)
						+ String.Format("  {0}</dt> ", opt.Text));

					i++;
				}
			}

			formFld = new MvcHtmlString(sbChk.ToString());

			return formFld;
		}

		private string GetInputName(MethodCallExpression expression) {
			var methodCallExpression = expression.Object as MethodCallExpression;
			if (methodCallExpression != null) {
				return GetInputName(methodCallExpression);
			}
			return expression.Object.ToString();
		}

		public IHtmlString OutputHtmlBody() {
			if (this.DataPage != null && this.DataPage.DataSource != null && this.DataPage.DataSource.Any()) {
				return CreateBody();
			} else {
				return EmptyTable();
			}
		}

		protected IHtmlString CreateBody() {
			StringBuilder sb = new StringBuilder();
			this.RowNumber = 0;

			Type myType = typeof(T);
			_sortDir = this.DataPage.ParseSort();

			string frm = "form:first";
			if (!String.IsNullOrEmpty(this.HtmlFormId)) {
				frm = String.Format("#{0}", this.HtmlFormId);
			}

			if (this.UseDataPage) {
				sb.AppendLine(String.Empty);
				sb.AppendLine("	<script type=\"text/javascript\">");
				sb.AppendLine("	function __clickHead(fld) {");
				sb.AppendLine(String.Format("		$('#{0}SortByNew').val(fld);", this.FieldIdPrefix));
				sb.AppendLine(String.Format("		$('{0}')[0].submit();", frm));
				sb.AppendLine("	}");
				sb.AppendLine(String.Empty);
				sb.AppendLine("	function __clickPage(nbr, fld) {");
				sb.AppendLine("		$('#' + fld).val(nbr);");
				sb.AppendLine("		$('#' + fld).focus();");
				sb.AppendLine(String.Format("		$('{0}')[0].submit();", frm));
				sb.AppendLine("	}");
				sb.AppendLine("	</script>");
				sb.AppendLine(String.Empty);

				FormHelper(x => x.OrderBy, sb);
				FormHelper(x => x.SortByNew, sb);
				if (!this.PageSizeExternal) {
					FormHelper(x => x.PageSize, sb);
				}
				FormHelper(x => x.TotalRecords, sb);
				FormHelper(x => x.MaxPage, sb);
				FormHelper(x => x.PageNumber, sb);
			}

			IDictionary<string, object> tblAttrib = InitAttrib(this.TableAttributes);

			tblAttrib.Add("id", this.HtmlClientId);

			using (new WrappedItem(sb, "table", tblAttrib)) {
				using (new WrappedItem(sb, "thead", this.THeadAttributes)) {
					using (new WrappedItem(sb, "tr", null)) {
						foreach (var col in this.Columns) {
							using (new WrappedItem(sb, "th", col.HeadAttributes)) {
								if (col is ICarrotGridColumnExt) {
									var colExt = (ICarrotGridColumnExt)col;
									if (colExt.Sortable && this.UseDataPage) {
										string js = String.Format("javascript:__clickHead('{0}')", colExt.ColumnName);

										IDictionary<string, object> tagAttrib = InitAttrib(colExt.HeadLinkAttributes);

										tagAttrib.Add("href", js);

										using (new WrappedItem(sb, "a", tagAttrib)) {
											sb.Append(col.HeaderText);

											if (_sortDir.SortField.ToUpper() == colExt.ColumnName.ToUpper()) {
												if (_sortDir.SortDirection.ToUpper() == "ASC") {
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

				var url = new UrlHelper(_htmlHelper.ViewContext.RequestContext);

				using (new WrappedItem(sb, "tbody", this.TBodyAttributes)) {
					foreach (var row in this.DataPage.DataSource) {
						using (new WrappedItem(sb, "tr", new { rowNbr = this.RowNumber })) {
							foreach (var col in this.Columns) {
								using (new WrappedItem(sb, "td", col.BodyAttributes)) {
									string cellContents = String.Empty;

									if (col is ICarrotGridColumnExt) {
										var colExt = (ICarrotGridColumnExt)col;
										Object val = row.GetPropValueFromColumnName(colExt.ColumnName);

										string imgPath = String.Empty;
										switch (col.Mode) {
											case CarrotGridColumnType.Standard:

												cellContents = String.Format(colExt.CellFormatString, val);
												break;

											case CarrotGridColumnType.ImageEnum:
												CarrotImageColumnData imgData = null;

												CarrotGridImageColumn ic = (CarrotGridImageColumn)col;
												imgPath = ic.DefaultImagePath;
												string key = val.ToString();

												if (ic.ImagePairs.Where(x => x.KeyValue == key).Any()) {
													imgData = ic.ImagePairs.Where(x => x.KeyValue == key).FirstOrDefault();
													imgPath = imgData.ImagePath;
												}

												if (ic is CarrotGridImageColumn) {
													string imageAltText = String.Format(colExt.CellFormatString, val);
													if (imgData != null) {
														imageAltText = imgData.ImageAltText;
													}

													var imgBuilder = new TagBuilder("img");
													imgBuilder.MergeAttribute("src", url.Content(imgPath));
													imgBuilder.MergeAttribute("alt", imageAltText);
													imgBuilder.MergeAttribute("title", imageAltText);

													var imgAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(ic.ImageAttributes);
													imgBuilder.MergeAttributes(imgAttribs);

													cellContents = imgBuilder.ToString(TagRenderMode.SelfClosing);
												}
												break;

											case CarrotGridColumnType.BooleanImage:

												CarrotGridBooleanImageColumn bic = (CarrotGridBooleanImageColumn)col;
												if (bic is CarrotGridBooleanImageColumn) {
													bool imageState = false;
													imgPath = bic.ImagePathFalse;

													if (val.GetType() == typeof(bool) && (bool)val) {
														imgPath = bic.ImagePathTrue;
														imageState = true;
													}

													string sTxt = imageState ? bic.AlternateTextTrue : bic.AlternateTextFalse;

													var imgBuilder = new TagBuilder("img");
													imgBuilder.MergeAttribute("src", url.Content(imgPath));
													imgBuilder.MergeAttribute("alt", sTxt);
													imgBuilder.MergeAttribute("title", sTxt);

													var imgAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(bic.ImageAttributes);
													imgBuilder.MergeAttributes(imgAttribs);

													cellContents = imgBuilder.ToString(TagRenderMode.SelfClosing);
												}

												break;

											default:
												break;
										}
									}

									if (col is ICarrotGridColumnTemplate<T> && col.Mode == CarrotGridColumnType.Template) {
										var colTmpl = (ICarrotGridColumnTemplate<T>)col;
										if (colTmpl.FormatTemplate != null) {
											cellContents = (new HelperResult(writer => {
												colTmpl.FormatTemplate(row).WriteTo(writer);
											})).ToHtmlString();
										}
									}

									sb.Append(cellContents);
								}
							}
						}

						this.RowNumber++;
						sb.AppendLine();
					}
				}
			}

			return new HtmlString(sb.ToString());
		}

		protected IHtmlString EmptyTable() {
			this.DataPage.TotalRecords = 0;
			this.DataPage.PageNumber = 1;

			string cellContents = String.Empty;

			StringBuilder sb = new StringBuilder();

			FormHelper(x => x.OrderBy, sb);
			FormHelper(x => x.SortByNew, sb);
			if (!this.PageSizeExternal) {
				FormHelper(x => x.PageSize, sb);
			}
			FormHelper(x => x.TotalRecords, sb);
			FormHelper(x => x.MaxPage, sb);
			FormHelper(x => x.PageNumber, sb);

			if ((this.DataPage.DataSource == null || !this.DataPage.DataSource.Any()) && this.EmptyDataTemplate != null) {
				cellContents = (new HelperResult(writer => {
					this.EmptyDataTemplate(new Object()).WriteTo(writer);
				})).ToHtmlString();
			}

			sb.AppendLine(cellContents);

			return new HtmlString(sb.ToString());
		}

		public void SetupFooter(string outer, object outerAttrib, string inner, object selAttrib, object noselAttrib) {
			this.FooterOuterTag = String.IsNullOrEmpty(outer) ? "ul" : outer;

			this.htmlFootAttrib = outerAttrib;

			this.FooterTag = String.IsNullOrEmpty(inner) ? "li" : inner;

			this.htmlFootSel = selAttrib;
			this.htmlFootNotSel = noselAttrib;
		}

		public IHtmlString OutputFooter() {
			StringBuilder sb = new StringBuilder();

			if (this.DataPage.TotalPages > 1) {
				using (new WrappedItem(sb, this.FooterOuterTag, this.htmlFootAttrib)) {
					foreach (var i in this.DataPage.PageNumbers) {
						string clickFn = String.Format("javascript:__clickPage('{0}','{1}PageNumber')", i, this.FieldIdPrefix);

						using (new WrappedItem(_htmlHelper, sb, this.FooterTag, i, this.DataPage.PageNumber, this.htmlFootSel, this.htmlFootNotSel)) {
							using (new WrappedItem(sb, "a", new { @href = clickFn })) {
								sb.Append(String.Format(" {0} ", i));
							}
						}
					}
				}
			}

			return new HtmlString(sb.ToString());
		}

		public string ToHtmlString() {
			StringBuilder sb = new StringBuilder();
			//sb.AppendLine("<div>");
			sb.AppendLine(this.OutputHtmlBody().ToString());
			sb.AppendLine(this.OutputFooter().ToString());
			//sb.AppendLine("</div>");
			return sb.ToString();
		}

		public IHtmlString Write() {
			return new HtmlString(ToHtmlString());
		}
	}
}