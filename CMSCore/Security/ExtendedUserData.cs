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
* Dual licensed under the MIT or GPL Version 3 licenses.
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
		public string EditorURL { get { return ContentPageHelper.ScrubFilename(this.UserId, string.Format("/{0}/{1}", SiteData.CurrentSite.BlogEditorFolderPath, this.UserName)); } }

		public override string ToString() {
			return this.FullName_FirstLast;
		}

		[Display(Name = "First + Last")]
		public string FullName_FirstLast {
			get {
				if (!string.IsNullOrEmpty(this.LastName)) {
					return string.Format("{0} {1}", this.FirstName, this.LastName);
				} else {
					if (!string.IsNullOrEmpty(this.UserName)) {
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
				if (!string.IsNullOrEmpty(this.LastName)) {
					return string.Format("{0}, {1}", this.LastName, this.FirstName);
				} else {
					if (!string.IsNullOrEmpty(this.UserName)) {
						return this.UserName;
					} else {
						return "Unknown User";
					}
				}
			}
		}

		public ExtendedUserData() { }

		public ExtendedUserData(string UserName) {
			using (var db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByName(db, UserName);
				LoadUserData(rc);
			}
		}

		public ExtendedUserData(Guid UserID) {
			using (var db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByID(db, UserID);
				LoadUserData(rc);
			}
		}

		public static ExtendedUserData FindByUsername(string UserName) {
			ExtendedUserData usr = new ExtendedUserData();

			using (var db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByName(db, UserName);
				usr.LoadUserData(rc);
			}

			return usr;
		}

		public static ExtendedUserData FindByEmail(string Email) {
			ExtendedUserData usr = new ExtendedUserData();

			using (var db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByEmail(db, Email);
				usr.LoadUserData(rc);
			}

			return usr;
		}

		public static ExtendedUserData FindByUserID(Guid UserID) {
			ExtendedUserData usr = new ExtendedUserData();

			using (var db = CarrotCMSDataContext.Create()) {
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByID(db, UserID);
				usr.LoadUserData(rc);
			}

			return usr;
		}

		internal void LoadUserData(vw_carrot_UserData c) {
			this.UserId = Guid.Empty;
			this.Email = string.Empty;
			this.UserName = string.Empty;
			this.UserKey = string.Empty;

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
					using (var db = CarrotCMSDataContext.Create()) {
						_siteIDs = (from m in db.carrot_UserSiteMappings
									where m.UserId == this.UserId
									select m.SiteID).Distinct().ToList();
					}
				}
				return _siteIDs;
			}
		}

		public List<SiteData> GetSiteList() {
			using (var db = CarrotCMSDataContext.Create()) {
				return (from m in db.carrot_UserSiteMappings
						join s in db.carrot_Sites on m.SiteID equals s.SiteID
						where m.UserId == this.UserId
						select new SiteData(s)).ToList();
			}
		}

		public List<UserRole> GetRoles() {
			using (var db = CarrotCMSDataContext.Create()) {
				return (from ur in db.membership_UserRoles
						join u in db.membership_Users on ur.UserId equals u.Id
						join r in db.membership_Roles on ur.RoleId equals r.Id
						join ud in db.carrot_UserDatas on u.Id equals ud.UserKey
						where u.UserName == this.UserName
						orderby r.Name
						select new UserRole(r)).ToList();
			}
		}

		public bool AddToRole(string roleName) {
			return SecurityData.AddUserToRole(this.UserName, roleName);
		}

		public bool RemoveFromRole(string roleName) {
			return SecurityData.RemoveUserFromRole(this.UserName, roleName);
		}

		public bool AddToSite(Guid siteID) {
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_UserSiteMapping map = (from m in db.carrot_UserSiteMappings
											  where m.UserId == this.UserId
												&& m.SiteID == siteID
											  select m).FirstOrDefault();

				if (map == null) {
					_siteIDs = null;

					map = new carrot_UserSiteMapping();
					map.UserSiteMappingID = Guid.NewGuid();
					map.SiteID = siteID;
					map.UserId = this.UserId;

					db.carrot_UserSiteMappings.InsertOnSubmit(map);
					db.SubmitChanges();

					return true;
				} else {
					return false;
				}
			}
		}

		public bool RemoveFromSite(Guid siteID) {
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_UserSiteMapping map = (from m in db.carrot_UserSiteMappings
											  where m.UserId == this.UserId
												&& m.SiteID == siteID
											  select m).FirstOrDefault();

				if (map != null) {
					_siteIDs = null;

					db.carrot_UserSiteMappings.DeleteOnSubmit(map);
					db.SubmitChanges();

					return true;
				} else {
					return false;
				}
			}
		}

		public void Save() {
			using (var db = CarrotCMSDataContext.Create()) {
				bool bNew = false;
				carrot_UserData usr = CompiledQueries.cqFindUserTblByID(db, this.UserId);

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
					db.carrot_UserDatas.InsertOnSubmit(usr);
				}

				db.SubmitChanges();

				this.UserId = usr.UserId;

				//grab fresh copy from DB
				vw_carrot_UserData rc = CompiledQueries.cqFindUserByID(db, usr.UserId);
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
			using (var db = CarrotCMSDataContext.Create()) {
				List<ExtendedUserData> lstUsr = (from u in CompiledQueries.cqGetUserList(db)
												 select new ExtendedUserData(u)).ToList();
				return lstUsr;
			}
		}

		public static IQueryable<ExtendedUserData> GetUsers() {
			using (var db = CarrotCMSDataContext.Create()) {
				IQueryable<ExtendedUserData> lstUsr = (from u in CompiledQueries.cqGetUserList(db)
													   select new ExtendedUserData(u));
				return lstUsr;
			}
		}

		public static ExtendedUserData GetEditorFromURL() {
			using (var db = CarrotCMSDataContext.Create()) {
				vw_carrot_EditorURL query = CompiledQueries.cqGetEditorByURL(db, SiteData.CurrentSiteID, SiteData.CurrentScriptName);
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