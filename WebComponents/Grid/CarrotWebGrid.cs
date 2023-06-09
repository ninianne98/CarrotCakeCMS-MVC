using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

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

	public class CarrotWebGrid<T> : CarrotWebGridBase where T : class {
		public PagedData<T> DataPage { get; set; }

		public CarrotWebGrid(HtmlHelper htmlHelper)
			: this(htmlHelper, new PagedData<T>()) {
			this.UseDataPage = false;
		}

		public CarrotWebGrid(HtmlHelper htmlHelper, PagedData<T> dp) {
			base.StandardInit(htmlHelper, dp);

			this.HtmlClientId = "tbl" + typeof(T).Name;
			this.DataPage = dp;
			base.PagedDataBase = this.DataPage;
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

					if (string.IsNullOrEmpty(column.HeaderText) && column.HasHeadingText) {
						column.HeaderText = col.ColumnName.Replace(".", " ").Replace("_", " ");

						string displayName = CarrotWeb.DisplayNameFor<T>(property);
						if (!string.IsNullOrEmpty(displayName)) {
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

		protected string DataFieldName(string columnName) {
			string fldName = string.Format("{0}DataSource[{1}].{2}", this.FieldNamePrefix, this.RowNumber, columnName);
			if (!this.UseDataPage) {
				if (string.IsNullOrEmpty(this.FieldNamePrefix)) {
					//fldName = fldName.Replace(string.Format("DataSource[{0}]", this.RowNumber), string.Format("[{0}]", this.RowNumber));
					fldName = string.Format("[{0}].{1}", this.RowNumber, columnName);
				} else {
					//fldName = fldName.Replace(string.Format(".DataSource[{0}]", this.RowNumber), string.Format("[{0}]", this.RowNumber));
					fldName = string.Format("{0}[{1}].{2}", this.FieldNamePrefix, this.RowNumber, columnName).Replace(".[", "[");
				}
			}
			return fldName;
		}

		public MvcHtmlString FormFieldFor(Expression<Func<T, Object>> property, GridFormFieldType fldType, object htmlAttribs = null) {
			T row = this.DataPage.DataSource[this.RowNumber];

			//PropertyInfo propInfo = row.PropInfoFromExpression<T>(property);
			//object val = propInfo.GetValue(row, null);
			string columnName = ReflectionUtilities.BuildProp(property);
			object val = row.GetPropValueFromExpression(property);

			string fldName = DataFieldName(columnName);

			MvcHtmlString formFld = new MvcHtmlString(string.Empty);

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
			object val = row.GetPropValueFromExpression(property);

			string fldName = DataFieldName(columnName);

			MvcHtmlString formFld = new MvcHtmlString(string.Empty);

			if (val != null && selectList.SelectedValue == null) {
				selectList = new SelectList(selectList.Items, selectList.DataValueField, selectList.DataTextField, val);
			}

			if (!string.IsNullOrEmpty(optionLabel)) {
				formFld = _htmlHelper.DropDownList(fldName, selectList, optionLabel, htmlAttributes);
			} else {
				formFld = _htmlHelper.DropDownList(fldName, selectList, htmlAttributes);
			}

			return formFld;
		}

		public MvcHtmlString CheckBoxListFor(Expression<Func<T, Object>> property, MultiSelectList selectList, string selectedFieldName, object chkboxAttributes = null, object listAttributes = null) {
			T row = this.DataPage.DataSource[this.RowNumber];
			string columnName = string.Empty;
			selectedFieldName = string.IsNullOrEmpty(selectedFieldName) ? "Selected" : selectedFieldName;

			if (property.Body.NodeType == ExpressionType.Call) {
				var methodCallExpression = (MethodCallExpression)property.Body;
				columnName = GetInputName(methodCallExpression);
			} else {
				columnName = ReflectionUtilities.BuildProp(property);
			}

			string fldName = DataFieldName(columnName);

			MvcHtmlString formFld = new MvcHtmlString(string.Empty);

			StringBuilder sbChk = new StringBuilder();
			int i = 0;
			using (new WrappedItem(sbChk, "dl", listAttributes)) {
				foreach (var opt in selectList) {
					sbChk.AppendLine("<dt>"
						+ _htmlHelper.Hidden(string.Format("{0}[{1}].{2}", fldName, i, selectList.DataValueField), opt.Value)
						+ _htmlHelper.CheckBox(string.Format("{0}[{1}].{2}", fldName, i, selectedFieldName), opt.Selected, chkboxAttributes)
						+ string.Format("  {0}</dt> ", opt.Text));

					i++;
				}
			}

			formFld = new MvcHtmlString(sbChk.ToString());

			return formFld;
		}

		protected string GetInputName(MethodCallExpression expression) {
			var methodCallExpression = expression.Object as MethodCallExpression;
			if (methodCallExpression != null) {
				return GetInputName(methodCallExpression);
			}
			return expression.Object.ToString();
		}

		protected override IHtmlString CreateBody() {
			var sb = new StringBuilder();
			this.RowNumber = 0;

			_sortDir = this.DataPage.ParseSort();

			BuildHeadScript(sb);

			IDictionary<string, object> tblAttrib = InitAttrib(this.TableAttributes);

			tblAttrib.Add("id", this.HtmlClientId);

			using (new WrappedItem(sb, "table", tblAttrib)) {
				BuildTableHeadRow(sb);

				var url = new UrlHelper(_htmlHelper.ViewContext.RequestContext);

				using (new WrappedItem(sb, "tbody", this.TBodyAttributes)) {
					foreach (var row in this.DataPage.DataSource) {
						using (new WrappedItem(sb, "tr", new { rowNbr = this.RowNumber })) {
							foreach (var col in this.Columns) {
								using (new WrappedItem(sb, "td", col.BodyAttributes)) {
									string cellContents = string.Empty;

									if (col is ICarrotGridColumnExt) {
										var colExt = (ICarrotGridColumnExt)col;
										object val = row.GetPropValueFromColumnName(colExt.ColumnName);

										string imgPath = string.Empty;
										switch (col.Mode) {
											case CarrotGridColumnType.Standard:

												cellContents = string.Format(colExt.CellFormatString, val);
												break;

											case CarrotGridColumnType.ImageEnum:
												CarrotImageColumnData imgData = null;

												var ic = (CarrotGridImageColumn)col;
												imgPath = ic.DefaultImagePath;
												string key = val.ToString();

												if (ic.ImagePairs.Where(x => x.KeyValue == key).Any()) {
													imgData = ic.ImagePairs.Where(x => x.KeyValue == key).FirstOrDefault();
													imgPath = imgData.ImagePath;
												}

												if (ic is CarrotGridImageColumn) {
													string imageAltText = string.Format(colExt.CellFormatString, val);
													if (imgData != null) {
														imageAltText = imgData.ImageAltText;
													}

													var imgBuilder = new HtmlTag("img");
													imgBuilder.Uri = url.Content(imgPath);
													imgBuilder.MergeAttribute("alt", imageAltText);
													imgBuilder.MergeAttribute("title", imageAltText);
													imgBuilder.MergeAttributes(ic.ImageAttributes);

													cellContents = imgBuilder.RenderSelfClosingTag();
												}
												break;

											case CarrotGridColumnType.BooleanImage:

												var bic = (CarrotGridBooleanImageColumn)col;
												if (bic is CarrotGridBooleanImageColumn) {
													bool imageState = false;
													imgPath = bic.ImagePathFalse;

													if (val.GetType() == typeof(bool) && (bool)val) {
														imgPath = bic.ImagePathTrue;
														imageState = true;
													}

													string sTxt = imageState ? bic.AlternateTextTrue : bic.AlternateTextFalse;

													var imgBuilder = new HtmlTag("img");
													imgBuilder.Uri = url.Content(imgPath);
													imgBuilder.MergeAttribute("alt", sTxt);
													imgBuilder.MergeAttribute("title", sTxt);
													imgBuilder.MergeAttributes(bic.ImageAttributes);

													cellContents = imgBuilder.RenderSelfClosingTag();
												}

												break;

											default:
												break;
										}
									}

									if (col is ICarrotGridColumnTemplate<T> && col.Mode == CarrotGridColumnType.Template) {
										var colTmpl = (ICarrotGridColumnTemplate<T>)col;
										if (colTmpl.FormatTemplate != null) {
											cellContents = colTmpl.FormatTemplate(row).ToHtmlString();
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

		public override IHtmlString OutputHtmlBody() {
			base.PagedDataBase = this.DataPage;

			return base.OutputHtmlBody();
		}

		public override IHtmlString OutputFooter() {
			base.PagedDataBase = this.DataPage;

			return base.OutputFooter();
		}

		protected override IHtmlString EmptyTable() {
			base.PagedDataBase = this.DataPage;

			return base.EmptyTable();
		}
	}
}