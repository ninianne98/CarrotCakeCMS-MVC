using Carrotware.CMS.Interface;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
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

namespace Carrotware.CMS.Core {

	public class ObjectProperty {

		public ObjectProperty() { }

		public string Name { get; set; }
		public bool CanWrite { get; set; }
		public bool CanRead { get; set; }
		public Type PropertyType { get; set; }

		public Object DefValue { get; set; }

		public PropertyInfo Props { get; set; }

		public string CompanionSourceFieldName { get; set; }

		public string FieldDescription { get; set; }

		public WidgetAttribute.FieldMode FieldMode { get; set; }

		public List<OptionSelections> Options { get; set; }

		public bool CheckBoxState { get; set; }
		public string TextValue { get; set; }

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || this.GetType() != obj.GetType()) return false;
			if (obj is ObjectProperty) {
				ObjectProperty p = (ObjectProperty)obj;
				return (this.Name == p.Name) && (this.PropertyType == p.PropertyType);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.Name.GetHashCode() ^ this.PropertyType.ToString().GetHashCode();
		}

		//============================

		public static List<ObjectProperty> GetWidgetProperties(Widget w, Guid guidContentID) {
			Object widget = new Object();

			List<WidgetProps> lstProps = w.ParseDefaultControlProperties();

			if (w.ControlPath.Contains(":")) {
				if (w.ControlPath.ToUpper().StartsWith("CLASS:")) {
					try {
						string className = w.ControlPath.Replace("CLASS:", "");
						Type t = ReflectionUtilities.GetTypeFromString(className);
						widget = Activator.CreateInstance(t);
					} catch (Exception ex) { }
				} else {
					try {
						string[] path = w.ControlPath.Split(':');
						string objectPrefix = path[0];
						string objectClass = path[1];

						Type t = ReflectionUtilities.GetTypeFromString(objectClass);

						Object obj = Activator.CreateInstance(t);
						Object attrib = ReflectionUtilities.GetAttribute<WidgetActionSettingModelAttribute>(t, objectPrefix);

						if (attrib != null && attrib is WidgetActionSettingModelAttribute) {
							string attrClass = (attrib as WidgetActionSettingModelAttribute).ClassName;
							Type s = ReflectionUtilities.GetTypeFromString(attrClass);

							widget = Activator.CreateInstance(s);
						}
					} catch (Exception ex) { }
				}
			} else {
				if (w.ControlPath.Contains("|")) {
					try {
						string[] path = w.ControlPath.Split('|');
						string viewPath = path[0];
						string modelClass = String.Empty;
						if (path.Length > 1) {
							modelClass = path[1];
							Type objType = ReflectionUtilities.GetTypeFromString(modelClass);

							widget = Activator.CreateInstance(objType);
						}
					} catch (Exception ex) { }
				}
			}

			if (widget is IAdminModule) {
				var w1 = (IAdminModule)widget;
				w1.SiteID = SiteData.CurrentSiteID;
			}

			if (widget is IWidget) {
				var w1 = (IWidget)widget;
				w1.SiteID = SiteData.CurrentSiteID;
				w1.RootContentID = w.Root_ContentID;
				w1.PageWidgetID = w.Root_WidgetID;
				w1.IsDynamicInserted = true;
			}

			if (widget is IWidgetRawData) {
				var w1 = (IWidgetRawData)widget;
				w1.RawWidgetData = w.ControlProperties;
			}

			List<ObjectProperty> lstDefProps = ObjectProperty.GetObjectProperties(widget);

			//require that widget be attributed to be on the list
			List<string> limitedPropertyList = (from ww in widget.GetType().GetProperties()
												where Attribute.IsDefined(ww, typeof(WidgetAttribute))
												select ww.Name.ToLower()).ToList();

			List<ObjectProperty> lstPropsToEdit = (from p in lstDefProps
												   join l in limitedPropertyList on p.Name.ToLower() equals l.ToLower()
												   where p.CanRead == true
													   && p.CanWrite == true
												   select p).ToList();

			foreach (var dp in lstPropsToEdit) {
				string sName = dp.Name.ToLower();
				List<WidgetProps> lstItmVals = lstProps.Where(x => x.KeyName.ToLower().StartsWith(sName + "|") || x.KeyName.ToLower() == sName).ToList();

				ObjectProperty sourceProperty = new ObjectProperty();

				string sListSourcePropertyName = (from p in lstDefProps
												  where p.Name.ToLower() == sName.ToLower()
														&& !String.IsNullOrEmpty(p.CompanionSourceFieldName)
												  select p.CompanionSourceFieldName).FirstOrDefault();

				if (String.IsNullOrEmpty(sListSourcePropertyName)) {
					sListSourcePropertyName = String.Empty;
				}

				sourceProperty = (from p in lstDefProps
								  where p.CanRead == true
									 && p.CanWrite == false
									 && p.Name.ToLower() == sListSourcePropertyName.ToLower()
								  select p).FirstOrDefault();

				if (dp.FieldMode != WidgetAttribute.FieldMode.CheckBoxList) {
					string sDefTxt = String.Empty;

					if (lstItmVals != null && lstItmVals.Any()) {
						dp.TextValue = lstItmVals != null ? lstItmVals.FirstOrDefault().KeyValue : String.Empty;
					} else {
						if (dp.DefValue != null) {
							sDefTxt = dp.DefValue.ToString();

							if (dp.PropertyType == typeof(bool)) {
								bool vB = Convert.ToBoolean(dp.DefValue.ToString());
								sDefTxt = vB.ToString();
							}
							if (dp.PropertyType == typeof(System.Drawing.Color)) {
								System.Drawing.Color vC = (System.Drawing.Color)dp.DefValue;
								sDefTxt = System.Drawing.ColorTranslator.ToHtml(vC);
							}
						}

						dp.TextValue = sDefTxt;
					}

					dp.DefValue = dp.DefValue;
				}

				Type pt = dp.PropertyType;

				if (sourceProperty != null) {
					if (sourceProperty.DefValue is Dictionary<string, string>) {
						dp.Options = OptionSelections.GetOptionsFromDictionary((Dictionary<string, string>)sourceProperty.DefValue);

						// work with a checkbox list, allow more than one value
						if (dp.FieldMode == WidgetAttribute.FieldMode.CheckBoxList) {
							// since this is a multi selected capable field, look for anything that starts with the
							// field name and has the delimiter trailing

							if (lstItmVals.Any() && dp.Options.Any()) {
								foreach (var v in dp.Options) {
									v.Selected = (from p in lstItmVals
												  where p.KeyValue == v.Key
												  select p.KeyValue).Any();
								}
							}
						}
					}
				}

				if ((pt == typeof(bool)) || dp.FieldMode == WidgetAttribute.FieldMode.CheckBox) {
					dp.FieldMode = WidgetAttribute.FieldMode.CheckBox;
					dp.CheckBoxState = Convert.ToBoolean(dp.DefValue);
				}
			}

			return lstPropsToEdit;
		}

		public static List<ObjectProperty> GetObjectProperties(Object obj) {
			List<ObjectProperty> props = (from i in ReflectionUtilities.GetProperties(obj)
										  select GetCustProps(obj, i)).ToList();
			return props;
		}

		public static List<ObjectProperty> GetTypeProperties(Type theType) {
			List<ObjectProperty> props = (from i in ReflectionUtilities.GetProperties(theType)
										  select new ObjectProperty {
											  Name = i.Name,
											  PropertyType = i.PropertyType,
											  CanRead = i.CanRead,
											  CanWrite = i.CanWrite
										  }).ToList();
			return props;
		}

		public static ObjectProperty GetCustProps(Object obj, PropertyInfo prop) {
			ObjectProperty objprop = new ObjectProperty {
				Name = prop.Name,
				DefValue = obj.GetType().GetProperty(prop.Name).GetValue(obj, null),
				PropertyType = prop.PropertyType,
				CanRead = prop.CanRead,
				CanWrite = prop.CanWrite,
				Props = prop,
				CompanionSourceFieldName = String.Empty,
				FieldMode = prop.PropertyType == typeof(bool) ?
						WidgetAttribute.FieldMode.CheckBox : WidgetAttribute.FieldMode.TextBox
			};

			try {
				foreach (Attribute attr in objprop.Props.GetCustomAttributes(true)) {
					if (attr is WidgetAttribute) {
						var widgetAttrib = attr as WidgetAttribute;
						if (null != widgetAttrib) {
							try { objprop.CompanionSourceFieldName = widgetAttrib.SelectFieldSource; } catch { objprop.CompanionSourceFieldName = ""; }
							try { objprop.FieldMode = widgetAttrib.Mode; } catch { objprop.FieldMode = WidgetAttribute.FieldMode.Unknown; }
						}
					}
				}
			} catch (Exception ex) { }

			objprop.FieldDescription = ReflectionUtilities.GetDescriptionAttribute(obj.GetType(), objprop.Name);

			return objprop;
		}
	}

	//======================

	public class OptionSelections {

		// may change to using SelectListItem
		public OptionSelections() { }

		public string Key { get; set; }

		public string Value { get; set; }

		public bool Selected { get; set; }

		public static List<OptionSelections> GetOptionsFromDictionary(Dictionary<string, string> dic) {
			return (from d in dic
					select new OptionSelections {
						Key = d.Key,
						Value = d.Value
					}).ToList();
		}
	}
}