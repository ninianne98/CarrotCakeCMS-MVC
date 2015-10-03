using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class ExtendedUserData {

		[Display(Name = "ID")]
		public string Id { get; set; }

		[Display(Name = "Email")]
		[StringLength(128)]
		[Required]
		public string Email { get; set; }

		public bool EmailConfirmed { get; set; }
		public string PasswordHash { get; set; }
		public string SecurityStamp { get; set; }

		[Display(Name = "Phone #")]
		public string PhoneNumber { get; set; }

		public bool PhoneNumberConfirmed { get; set; }
		public bool TwoFactorEnabled { get; set; }

		[Display(Name = "Lockout End Date (UTC)")]
		public DateTime? LockoutEndDateUtc { get; set; }

		public bool LockoutEnabled { get; set; }

		[Display(Name = "No Lockout Date")]
		public bool LockoutEndDateBlank { get; set; }

		public int AccessFailedCount { get; set; }

		[Display(Name = "Username")]
		[Required]
		public string UserName { get; set; }

		[Display(Name = "User Id")]
		[Required]
		public Guid UserId { get; set; }

		public string UserKey { get; set; }

		[StringLength(128)]
		[Display(Name = "Nickname")]
		public string UserNickName { get; set; }

		[StringLength(128)]
		[Display(Name = "First")]
		public string FirstName { get; set; }

		[StringLength(128)]
		[Display(Name = "Last")]
		public string LastName { get; set; }

		[Display(Name = "Bio")]
		public string UserBio { get; set; }

		[Display(Name = "URL")]
		public string EditorURL { get { return SiteData.CurrentSite.BlogEditorFolderPath + this.UserName; } }

		public override string ToString() {
			return this.FullName_FirstLast;
		}

		[Display(Name = "First + Last")]
		public string FullName_FirstLast {
			get {
				if (!String.IsNullOrEmpty(this.LastName)) {
					return String.Format("{0} {1}", this.FirstName, this.LastName);
				} else {
					if (!String.IsNullOrEmpty(this.UserName)) {
						return this.UserName;
					} else {
						return "Unknown User";
					}
				}
			}
		}

		[Display(Name = "Last, First")]
		public string FullName_LastFirst {
			get {
				if (!String.IsNullOrEmpty(this.LastName)) {
					return String.Format("{0}, {1}", this.LastName, this.FirstName);
				} else {
					if (!String.IsNullOrEmpty(this.UserName)) {
						return this.UserName;
					} else {
						return "Unknown User";
					}
				}
			}
		}

		public ExtendedUserData() { }

		public ExtendedUserData(string UserName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByName(_db, UserName);
				LoadUserData(rc);
			}
		}

		public ExtendedUserData(Guid UserID) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByID(_db, UserID);
				LoadUserData(rc);
			}
		}

		public static ExtendedUserData FindByUsername(string UserName) {
			ExtendedUserData usr = new ExtendedUserData();

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByName(_db, UserName);
				usr.LoadUserData(rc);
			}

			return usr;
		}

		public static ExtendedUserData FindByEmail(string Email) {
			ExtendedUserData usr = new ExtendedUserData();

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByEmail(_db, Email);
				usr.LoadUserData(rc);
			}

			return usr;
		}

		public static ExtendedUserData FindByUserID(Guid UserID) {
			ExtendedUserData usr = new ExtendedUserData();

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByID(_db, UserID);
				usr.LoadUserData(rc);
			}

			return usr;
		}

		internal void LoadUserData(vw_carrot_UserData c) {
			this.UserId = Guid.Empty;
			this.Email = String.Empty;
			this.UserName = String.Empty;
			this.UserKey = String.Empty;

			if (c != null) {
				this.Id = c.Id;
				this.Email = c.Email;
				this.EmailConfirmed = c.EmailConfirmed;
				this.PasswordHash = c.PasswordHash;
				this.SecurityStamp = c.SecurityStamp;
				this.PhoneNumber = c.PhoneNumber;
				this.PhoneNumberConfirmed = c.PhoneNumberConfirmed;
				this.TwoFactorEnabled = c.TwoFactorEnabled;
				this.LockoutEndDateUtc = c.LockoutEndDateUtc;
				this.LockoutEndDateBlank = !c.LockoutEndDateUtc.HasValue;
				this.LockoutEnabled = c.LockoutEnabled;
				this.AccessFailedCount = c.AccessFailedCount;
				this.UserName = c.UserName;
				this.UserId = c.UserId.HasValue ? c.UserId.Value : Guid.Empty;
				this.UserKey = c.UserKey;
				this.UserNickName = c.UserNickName;
				this.FirstName = c.FirstName;
				this.LastName = c.LastName;
				this.UserBio = c.UserBio;
			}
		}

		private List<Guid> _siteIDs = null;

		public List<Guid> MemberSiteIDs {
			get {
				if (_siteIDs == null) {
					using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
						_siteIDs = (from m in _db.carrot_UserSiteMappings
									where m.UserId == this.UserId
									select m.SiteID).Distinct().ToList();
					}
				}
				return _siteIDs;
			}
		}

		public List<SiteData> GetSiteList() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from m in _db.carrot_UserSiteMappings
						join s in _db.carrot_Sites on m.SiteID equals s.SiteID
						where m.UserId == this.UserId
						select new SiteData(s)).ToList();
			}
		}

		public List<UserRole> GetRoles() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from ur in _db.membership_UserRoles
						join u in _db.membership_Users on ur.UserId equals u.Id
						join r in _db.membership_Roles on ur.RoleId equals r.Id
						join ud in _db.carrot_UserDatas on u.Id equals ud.UserKey
						where u.UserName == this.UserName
						orderby r.Name
						select new UserRole(r)).ToList();
			}
		}

		public bool AddToRole(string roleName) {
			if (!SecurityData.IsUserInRole(this.UserName, roleName)) {
				SecurityData.AddUserToRole(this.UserName, roleName);
				return true;
			} else {
				return false;
			}
		}

		public bool RemoveFromRole(string roleName) {
			if (SecurityData.IsUserInRole(this.UserName, roleName)) {
				SecurityData.RemoveUserFromRole(this.UserName, roleName);
				return true;
			} else {
				return false;
			}
		}

		public bool AddToSite(Guid siteID) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_UserSiteMapping map = (from m in _db.carrot_UserSiteMappings
											  where m.UserId == this.UserId
												&& m.SiteID == siteID
											  select m).FirstOrDefault();

				if (map == null) {
					_siteIDs = null;

					map = new carrot_UserSiteMapping();
					map.UserSiteMappingID = Guid.NewGuid();
					map.SiteID = siteID;
					map.UserId = this.UserId;

					_db.carrot_UserSiteMappings.InsertOnSubmit(map);
					_db.SubmitChanges();

					return true;
				} else {
					return false;
				}
			}
		}

		public bool RemoveFromSite(Guid siteID) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_UserSiteMapping map = (from m in _db.carrot_UserSiteMappings
											  where m.UserId == this.UserId
												&& m.SiteID == siteID
											  select m).FirstOrDefault();

				if (map != null) {
					_siteIDs = null;

					_db.carrot_UserSiteMappings.DeleteOnSubmit(map);
					_db.SubmitChanges();

					return true;
				} else {
					return false;
				}
			}
		}

		public void Save() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				bool bNew = false;
				carrot_UserData usr = CompiledQueries.cqFindUserTblByID(_db, this.UserId);

				if (usr == null) {
					usr = new carrot_UserData();
					usr.UserKey = this.UserKey;
					usr.UserId = Guid.NewGuid();
					bNew = true;
				}

				usr.UserNickName = this.UserNickName;
				usr.FirstName = this.FirstName;
				usr.LastName = this.LastName;
				usr.UserBio = this.UserBio;

				if (bNew) {
					_db.carrot_UserDatas.InsertOnSubmit(usr);
				}

				_db.SubmitChanges();

				this.UserId = usr.UserId;

				//grab fresh copy from DB
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByID(_db, usr.UserId);
				LoadUserData(rc);
			}
		}

		internal ExtendedUserData(vw_carrot_UserData c) {
			LoadUserData(c);
		}

		[Display(Name = "Is Admin")]
		public bool IsAdmin {
			get {
				try {
					return SecurityData.IsUserInRole(this.UserName, SecurityData.CMSGroup_Admins);
				} catch (Exception ex) {
					return false;
				}
			}
		}

		[Display(Name = "Is Editor")]
		public bool IsEditor {
			get {
				try {
					return SecurityData.IsUserInRole(this.UserName, SecurityData.CMSGroup_Editors);
				} catch (Exception ex) {
					return false;
				}
			}
		}

		//================================================

		public static List<ExtendedUserData> GetUserList() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				List<ExtendedUserData> lstUsr = (from u in CompiledQueries.cqGetUserList(_db)
												 select new ExtendedUserData(u)).ToList();
				return lstUsr;
			}
		}

		public static IQueryable<ExtendedUserData> GetUsers() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				IQueryable<ExtendedUserData> lstUsr = (from u in CompiledQueries.cqGetUserList(_db)
													   select new ExtendedUserData(u));
				return lstUsr;
			}
		}

		public static ExtendedUserData GetEditorFromURL() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				vw_carrot_EditorURL query = CompiledQueries.cqGetEditorByURL(_db, SiteData.CurrentSiteID, SiteData.CurrentScriptName);
				if (query != null) {
					ExtendedUserData usr = new ExtendedUserData(query.UserId);
					return usr;
				} else {
					return null;
				}
			}
		}
	}
}