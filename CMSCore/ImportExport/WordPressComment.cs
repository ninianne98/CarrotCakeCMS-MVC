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

namespace Carrotware.CMS.Core {

	public class WordPressComment {

		public WordPressComment() { }

		public Guid ImportRootID { get; set; }
		public int PostID { get; set; }
		public int CommentID { get; set; }
		public string Author { get; set; }
		public string AuthorEmail { get; set; }
		public string AuthorURL { get; set; }
		public string AuthorIP { get; set; }
		public DateTime CommentDateUTC { get; set; }
		public string CommentContent { get; set; }
		public string Approved { get; set; }
		public string Type { get; set; }
	}
}