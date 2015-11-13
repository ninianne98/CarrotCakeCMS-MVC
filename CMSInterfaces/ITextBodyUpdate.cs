/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public interface ITextBodyUpdate {

		string UpdateContent(string TextContent);

		string UpdateContentPlainText(string TextContent);

		string UpdateContentRichText(string TextContent);

		string UpdateContentComment(string TextContent);

		string UpdateContentSnippet(string TextContent);
	}
}