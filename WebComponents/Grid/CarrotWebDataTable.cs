using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Web;
using System;

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

	public class CarrotWebDataTable : CarrotWebGridBase {
		public PagedDataTable DataPage { get; set; }

		public bool AutoGenerateColumns { get; set; }

		public bool AutoSort { get; set; }

		public CarrotWebDataTable(HtmlHelper htmlHelper)
			: this(htmlHelper, new PagedDataTable()) {
			this.AutoGenerateColumns = false;
			this.UseDataPage = false;
			this.AutoSort = true;
		}

		public CarrotWebDataTable(HtmlHelper htmlHelper, PagedDataTable dp) {
			base.StandardInit(htmlHelper, dp);

			this.AutoGenerateColumns = false;
			this.AutoSort = true;
			this.HtmlClientId = "tblDataTable";
			this.DataPage = dp;
			base.PagedDataBase = this.DataPage;
		}

		public CarrotWebDataTable AddColumn(string columnName) {
			AddColumn(columnName, new CarrotGridColumn());

			return this;
		}

		public CarrotWebDataTable AddColumn(string columnName, bool sortable) {
			AddColumn(columnName, new CarrotGridColumn { Sortable = sortable });

			return this;
		}

		public CarrotWebDataTable AddColumn(string columnName, ICarrotGridColumn column) {
			if (column.Mode != CarrotGridColumnType.Template) {
				if (column is ICarrotGridColumnExt) {
					var col = (ICarrotGridColumnExt)column;
					col.ColumnName = columnName;
					if (!this.UseDataPage) {
						col.Sortable = false;
					}

					if (string.IsNullOrEmpty(column.HeaderText) && column.HasHeadingText) {
						column.HeaderText = col.ColumnName.Replace(".", " ").Replace("_", " ");
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

		public CarrotWebDataTable AddColumn(CarrotGridTableTemplateColumn column) {
			column.Order = this.Columns.Count();

			this.Columns.Add(column);

			return this;
		}

		/*
		protected string DataFieldName(string columnName) {
			string fldName = string.Format("{0}DataSource.Rows[{1}][\"{2}\"]", this.FieldNamePrefix, this.RowNumber, columnName);
			if (!this.UseDataPage) {
				if (string.IsNullOrEmpty(this.FieldNamePrefix)) {
					fldName = string.Format("Rows[{0}][\"{1}\"]", this.RowNumber, columnName);
				} else {
					fldName = string.Format("{0}.Rows[{1}][\"{2}\"]", this.FieldNamePrefix, this.RowNumber, columnName).Replace(".[", "[");
				}
			}
			return fldName;
		}

		public MvcHtmlString FormFieldFor(string columnName, GridFormFieldType fldType, object htmlAttribs = null) {
			DataRow row = this.DataPage.DataSource.Rows[this.RowNumber];

			Object val = row[columnName];

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

		public MvcHtmlString DropDownFor(string columnName, SelectList selectList, string optionLabel, object htmlAttributes = null) {
			DataRow row = this.DataPage.DataSource.Rows[this.RowNumber];

			Object val = row[columnName];

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

		public MvcHtmlString CheckBoxListFor(string columnName, MultiSelectList selectList, string selectedFieldName, object chkboxAttributes = null, object listAttributes = null) {
			DataRow row = this.DataPage.DataSource.Rows[this.RowNumber];

			selectedFieldName = string.IsNullOrEmpty(selectedFieldName) ? "Selected" : selectedFieldName;

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
	 */

		protected override IHtmlString CreateBody() {
			StringBuilder sb = new StringBuilder();
			this.RowNumber = 0;

			if (this.AutoGenerateColumns) {
				foreach (DataColumn c in this.DataPage.DataSource.Columns) {
					if (c.DataType == typeof(bool)) {
						AddColumn(c.ColumnName, new CarrotGridBooleanImageColumn {
							Sortable = this.AutoSort
						});
					} else {
						AddColumn(c.ColumnName, this.AutoSort);
					}
				}
			}

			_sortDir = this.DataPage.ParseSort();

			BuildHeadScript(sb);

			IDictionary<string, object> tblAttrib = InitAttrib(this.TableAttributes);

			tblAttrib.Add("id", this.HtmlClientId);

			using (new WrappedItem(sb, "table", tblAttrib)) {
				BuildTableHeadRow(sb);

				var url = new UrlHelper(_htmlHelper.ViewContext.RequestContext);

				using (new WrappedItem(sb, "tbody", this.TBodyAttributes)) {
					foreach (DataRow row in this.DataPage.DataSource.Rows) {
						using (new WrappedItem(sb, "tr", new { rowNbr = this.RowNumber })) {
							foreach (var col in this.Columns) {
								using (new WrappedItem(sb, "td", col.BodyAttributes)) {
									string cellContents = string.Empty;

									if (col is ICarrotGridColumnExt) {
										var colExt = (ICarrotGridColumnExt)col;
										Object val = row[colExt.ColumnName];

										string imgPath = string.Empty;
										switch (col.Mode) {
											case CarrotGridColumnType.Standard:

												cellContents = string.Format(colExt.CellFormatString, val);
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
													string imageAltText = string.Format(colExt.CellFormatString, val);
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

									if (col is ICarrotGridColumnTemplate<DataRow> && col.Mode == CarrotGridColumnType.Template) {
										var colTmpl = (ICarrotGridColumnTemplate<DataRow>)col;
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