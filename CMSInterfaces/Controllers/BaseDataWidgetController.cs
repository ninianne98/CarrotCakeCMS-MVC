using System;

namespace Carrotware.CMS.Interface.Controllers {

	public class BaseDataWidgetController : BaseWidgetController, IWidgetDataObject {
		public virtual Object WidgetPayload { get; set; }
	}
}