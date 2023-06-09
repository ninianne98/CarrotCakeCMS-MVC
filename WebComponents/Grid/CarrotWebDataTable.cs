using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

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

		protected override IHtmlString CreateBody() {
			var sb = new StringBuilder();
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
										object val = row[colExt.ColumnName];

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

									if (col is ICarrotGridColumnTemplate<DataRow> && col.Mode == CarrotGridColumnType.Template) {
										var colTmpl = (ICarrotGridColumnTemplate<DataRow>)col;
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