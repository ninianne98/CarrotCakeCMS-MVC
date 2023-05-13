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

		string UpdateContent(string textContent);

		string UpdateContentPlainText(string textContent);

		string UpdateContentRichText(string textContent);

		string UpdateContentComment(string textContent);

		string UpdateContentSnippet(string textContent);
	}
}