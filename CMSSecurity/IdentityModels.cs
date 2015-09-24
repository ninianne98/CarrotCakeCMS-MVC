using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Carrotware.CMS.Security.Models {

	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	public class ApplicationUser : IdentityUser {

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager) {
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
			// Add custom user claims here
			return userIdentity;
		}
	}

	public class SecurityDbContext : IdentityDbContext<ApplicationUser> {

		public SecurityDbContext()
			: base("CarrotwareCMSConnectionString", throwIfV1Schema: false) {
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder); // This needs to go before the other rules!

			modelBuilder.Entity<ApplicationUser>().ToTable("membership_User");
			modelBuilder.Entity<IdentityRole>().ToTable("membership_Role");
			modelBuilder.Entity<IdentityUserRole>().ToTable("membership_UserRole");
			modelBuilder.Entity<IdentityUserClaim>().ToTable("membership_UserClaim");
			modelBuilder.Entity<IdentityUserLogin>().ToTable("membership_UserLogin");
		}

		public static SecurityDbContext Create() {
			return new SecurityDbContext();
		}
	}
}