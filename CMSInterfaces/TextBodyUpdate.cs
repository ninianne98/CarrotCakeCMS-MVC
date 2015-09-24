/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public abstract class TextBodyUpdate : ITextBodyUpdate {

		public virtual string UpdateContent(string TextContent) {
			return TextContent;
		}

		public virtual string UpdateContentPlainText(string TextContent) {
			return TextContent;
		}

		public virtual string UpdateContentRichText(string TextContent) {
			return TextContent;
		}

		public virtual string UpdateContentComment(string TextContent) {
			return TextContent;
		}

		public virtual string UpdateContentSnippet(string TextContent) {
			return TextContent;
		}
	}
}