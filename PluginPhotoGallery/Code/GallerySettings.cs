using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CarrotCake.CMS.Plugins.PhotoGallery {

	public class GallerySettings : WidgetActionSettingModel {

		public override bool EnableEdit {
			get {
				return true;
			}
		}

		[Description("Display gallery heading")]
		[Widget]
		public bool ShowHeading { get; set; }

		[Description("Scale gallery images")]
		[Widget]
		public bool ScaleImage { get; set; }

		[Description("Gallery to display")]
		[Widget(WidgetAttribute.FieldMode.DropDownList, "lstGalleryID")]
		public Guid GalleryID { get; set; }

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstGalleryID {
			get {
				//if (SiteID == Guid.Empty) {
				//	SiteID = SiteData.CurrentSiteID;
				//}
				Dictionary<string, string> _dict = null;

				GalleryHelper gh = new GalleryHelper(SiteID);

				_dict = (from c in gh.GalleryGroupListGetBySiteID()
						 orderby c.GalleryTitle
						 where c.SiteID == SiteID
						 select c).ToList().ToDictionary(k => k.GalleryID.ToString(), v => v.GalleryTitle);

				return _dict;
			}
		}

		[Description("Gallery image pixel height/width")]
		[Widget(WidgetAttribute.FieldMode.DropDownList, "lstSizes")]
		public int ThumbSize { get; set; }

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstSizes {
			get {
				Dictionary<string, string> _dict = new Dictionary<string, string>();

				_dict.Add("25", "25px");
				_dict.Add("50", "50px");
				_dict.Add("75", "75px");
				_dict.Add("100", "100px");
				_dict.Add("125", "125px");
				_dict.Add("150", "150px");
				_dict.Add("175", "175px");
				_dict.Add("200", "200px");
				_dict.Add("225", "225px");
				_dict.Add("250", "250px");

				return _dict;
			}
		}

		[Description("Gallery appearance (pretty photo skin)")]
		[Widget(WidgetAttribute.FieldMode.DropDownList, "lstPrettySkins")]
		public string PrettyPhotoSkin { get; set; }

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstPrettySkins {
			get {
				Dictionary<string, string> _dict = new Dictionary<string, string>();
				_dict.Add("pp_default", "default");
				_dict.Add("light_square", "light square");
				_dict.Add("light_rounded", "light rounded");
				_dict.Add("facebook", "facebook");
				_dict.Add("dark_square", "dark square");
				_dict.Add("dark_rounded", "dark rounded");
				return _dict;
			}
		}

		public override void LoadData() {
			base.LoadData();

			try {
				string sFoundVal = this.GetParmValue("GalleryID", Guid.Empty.ToString());

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.GalleryID = new Guid(sFoundVal);
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("ShowHeading", "false");

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.ShowHeading = Convert.ToBoolean(sFoundVal);
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("ScaleImage", "false");

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.ScaleImage = Convert.ToBoolean(sFoundVal);
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValueDefaultEmpty("ThumbSize", "150");

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.ThumbSize = Convert.ToInt32(sFoundVal);
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("PrettyPhotoSkin", "light_rounded");

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.PrettyPhotoSkin = sFoundVal;
				}
			} catch (Exception ex) { }
		}
	}
}