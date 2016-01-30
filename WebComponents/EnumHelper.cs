using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

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

	public class EnumHelper {

		public static List<EnumProperty> ToList<T>()
								 where T : struct, IConvertible, IComparable, IFormattable {
			List<EnumProperty> lst = new List<EnumProperty>();

			foreach (var o in Get<T>()) {
				int v = Convert.ToInt32(o, CultureInfo.InvariantCulture);
				string s = Enum.GetName(typeof(T), o);
				string d = GetDescription<T>(s);

				lst.Add(new EnumProperty(v, s, d));
			}

			return lst;
		}

		public static IEnumerable<T> Get<T>() {
			return System.Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static IEnumerable<T> GetAllValues<T>()
								 where T : struct, IConvertible, IComparable, IFormattable {
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static IEnumerable<T> GetAllNames<T>()
						 where T : struct, IConvertible, IComparable, IFormattable {
			return Enum.GetNames(typeof(T)).Cast<T>();
		}

		public static string GetDescription<T>(string memberName) {
			Type type = typeof(T);

			MemberInfo[] memInfo = type.GetMember(memberName);

			if (memInfo != null && memInfo.Length > 0) {
				object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attrs != null && attrs.Length > 0) {
					return ((DescriptionAttribute)attrs[0]).Description;
				}
			}

			//default to member name if no description
			return memberName;
		}
	}

	//=========================

	public class EnumProperty {

		public EnumProperty() { }

		public EnumProperty(int v, string t, string d) {
			this.Value = v;
			this.Text = t;
			this.Description = d;
		}

		public long Value { get; set; }

		public string Text { get; set; }

		public string Description { get; set; }
	}
}