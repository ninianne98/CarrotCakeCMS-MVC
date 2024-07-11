using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarrotCake.CMS.Plugins.PhotoGallery.Models {

	public class GalleryModel {

		public GalleryModel() {
			this.Images = new List<GalleryImageEntry>();
			this.InstanceId = "gallery";
			this.PrettyPhotoSkin = "light_rounded";
		}

		[Display(Name = "Gallery")]
		public GalleryGroup Gallery { get; set; }

		public List<GalleryImageEntry> Images { get; set; }

		public string InstanceId { get; set; }

		[Display(Name = "Pretty Photo Skin")]
		public string PrettyPhotoSkin { get; set; }

		public int ThumbSize { get; set; } = 100;

		public Guid GalleryID { get; set; }

		public bool ShowHeading { get; set; }

		public bool ScaleImage { get; set; } = true;
	}
}