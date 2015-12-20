﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CarrotCake.CMS.Plugins.CalendarModule.Code
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="CarrotCakeMVC")]
	public partial class dbCalendarDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InserttblCalendar(tblCalendar instance);
    partial void UpdatetblCalendar(tblCalendar instance);
    partial void DeletetblCalendar(tblCalendar instance);
    #endregion
		
		public dbCalendarDataContext() : 
				base(global::System.Configuration.ConfigurationManager.ConnectionStrings["CarrotwareCMSConnectionString"].ConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public dbCalendarDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public dbCalendarDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public dbCalendarDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public dbCalendarDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<tblCalendar> tblCalendars
		{
			get
			{
				return this.GetTable<tblCalendar>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.tblCalendar")]
	public partial class tblCalendar : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Guid _CalendarID;
		
		private System.Nullable<System.DateTime> _EventDate;
		
		private string _EventTitle;
		
		private string _EventDetail;
		
		private System.Nullable<bool> _IsActive;
		
		private System.Nullable<System.Guid> _SiteID;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnCalendarIDChanging(System.Guid value);
    partial void OnCalendarIDChanged();
    partial void OnEventDateChanging(System.Nullable<System.DateTime> value);
    partial void OnEventDateChanged();
    partial void OnEventTitleChanging(string value);
    partial void OnEventTitleChanged();
    partial void OnEventDetailChanging(string value);
    partial void OnEventDetailChanged();
    partial void OnIsActiveChanging(System.Nullable<bool> value);
    partial void OnIsActiveChanged();
    partial void OnSiteIDChanging(System.Nullable<System.Guid> value);
    partial void OnSiteIDChanged();
    #endregion
		
		public tblCalendar()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CalendarID", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
		public System.Guid CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				if ((this._CalendarID != value))
				{
					this.OnCalendarIDChanging(value);
					this.SendPropertyChanging();
					this._CalendarID = value;
					this.SendPropertyChanged("CalendarID");
					this.OnCalendarIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EventDate", DbType="DateTime")]
		public System.Nullable<System.DateTime> EventDate
		{
			get
			{
				return this._EventDate;
			}
			set
			{
				if ((this._EventDate != value))
				{
					this.OnEventDateChanging(value);
					this.SendPropertyChanging();
					this._EventDate = value;
					this.SendPropertyChanged("EventDate");
					this.OnEventDateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EventTitle", DbType="VarChar(255)")]
		public string EventTitle
		{
			get
			{
				return this._EventTitle;
			}
			set
			{
				if ((this._EventTitle != value))
				{
					this.OnEventTitleChanging(value);
					this.SendPropertyChanging();
					this._EventTitle = value;
					this.SendPropertyChanged("EventTitle");
					this.OnEventTitleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EventDetail", DbType="VarChar(MAX)")]
		public string EventDetail
		{
			get
			{
				return this._EventDetail;
			}
			set
			{
				if ((this._EventDetail != value))
				{
					this.OnEventDetailChanging(value);
					this.SendPropertyChanging();
					this._EventDetail = value;
					this.SendPropertyChanged("EventDetail");
					this.OnEventDetailChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsActive", DbType="Bit")]
		public System.Nullable<bool> IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				if ((this._IsActive != value))
				{
					this.OnIsActiveChanging(value);
					this.SendPropertyChanging();
					this._IsActive = value;
					this.SendPropertyChanged("IsActive");
					this.OnIsActiveChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SiteID", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				if ((this._SiteID != value))
				{
					this.OnSiteIDChanging(value);
					this.SendPropertyChanging();
					this._SiteID = value;
					this.SendPropertyChanged("SiteID");
					this.OnSiteIDChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591
