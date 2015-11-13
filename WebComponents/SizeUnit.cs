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

	public enum SizeType {
		Pixel,
		EM,
		Point
	}

	public class SizeUnit {

		public SizeUnit() {
			this.Value = null;
			this.Type = SizeType.Pixel;
		}

		public SizeUnit(string value) {
			this.Value = null;
			this.Type = SizeType.Pixel;

			if (!String.IsNullOrEmpty(value)) {
				value = value.ToLower();

				if (value.EndsWith("px")) {
					this.Type = SizeType.Pixel;
				}
				if (value.EndsWith("pt")) {
					this.Type = SizeType.Pixel;
				}
				if (value.EndsWith("em")) {
					this.Type = SizeType.EM;
				}

				this.Value = float.Parse(value.Replace("px", String.Empty).Replace("pt", String.Empty).Replace("em", String.Empty));
			}
		}

		public SizeUnit(double value) {
			this.Value = value;
			this.Type = SizeType.EM;
		}

		public SizeUnit(int value) {
			this.Value = value;
			this.Type = SizeType.Pixel;
		}

		public SizeUnit(double value, SizeType type) {
			this.Value = value;
			this.Type = type;
		}

		public SizeType Type { get; set; }

		public double? Value { get; set; }

		public override string ToString() {
			string val = String.Empty;

			if (this.Value.HasValue) {
				switch (this.Type) {
					case SizeType.Pixel:
						val = String.Format("{0:0}px", this.Value.Value);
						break;

					case SizeType.EM:
						val = String.Format("{0:0.00}em", this.Value.Value);
						break;

					case SizeType.Point:
						val = String.Format("{0:0.0}pt", this.Value.Value);
						break;

					default:
						val = String.Format("{0:0.0}", this.Value.Value);
						break;
				}
			}

			return val;
		}
	}
}