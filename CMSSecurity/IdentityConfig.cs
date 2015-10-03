using Carrotware.CMS.Security.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Carrotware.CMS.Security {

	public class EmailService : IIdentityMessageService {

		public Task SendAsync(IdentityMessage message) {
			// Plug in your email service here to send an email.
			return Task.FromResult(0);
		}
	}

	//====================================
	public class SmsService : IIdentityMessageService {

		public Task SendAsync(IdentityMessage message) {
			// Plug in your SMS service here to send a text message.
			return Task.FromResult(0);
		}
	}

	//====================================
	// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
	public class ApplicationUserManager : UserManager<ApplicationUser> {

		public ApplicationUserManager(IUserStore<ApplicationUser> store)
			: base(store) {
		}

		public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) {
			var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<SecurityDbContext>()));
			// Configure validation logic for usernames

			manager = Configure(manager);

			var dataProtectionProvider = options.DataProtectionProvider;
			if (dataProtectionProvider != null) {
				manager.UserTokenProvider =
					new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
			}
			return manager;
		}

		public static ApplicationUserManager Configure(ApplicationUserManager manager) {
			// Configure validation logic for usernames
			manager.UserValidator = new UserValidator<ApplicationUser>(manager) {
				AllowOnlyAlphanumericUserNames = true,
				RequireUniqueEmail = true
			};

			//TODO: make configurable
			// Configure validation logic for passwords
			manager.PasswordValidator = new PasswordValidator {
				RequiredLength = 6,
				RequireNonLetterOrDigit = true,
				RequireDigit = true,
				RequireLowercase = true,
				RequireUppercase = true,
			};

			//TODO: make configurable
			// Configure user lockout defaults
			manager.UserLockoutEnabledByDefault = true;
			manager.MaxFailedAccessAttemptsBeforeLockout = 5;
			manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(15);

			// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
			// You can write your own provider and plug it in here.
			manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser> {
				MessageFormat = "Your security code is {0}"
			});

			manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser> {
				Subject = "Security Code",
				BodyFormat = "Your security code is {0}"
			});

			manager.EmailService = new EmailService();
			manager.SmsService = new SmsService();

			return manager;
		}
	}

	//====================================
	// Configure the application sign-in manager which is used in this application.
	public class ApplicationSignInManager : SignInManager<ApplicationUser, string> {

		public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
			: base(userManager, authenticationManager) {
		}

		public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user) {
			return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
		}

		public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context) {
			return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
		}
	}

	//====================================
	public class ApplicationRoleManager : RoleManager<IdentityRole> {

		public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
			: base(roleStore) {
		}

		public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context) {
			var manager = new ApplicationRoleManager(
				new RoleStore<IdentityRole>(context.Get<SecurityDbContext>()));

			return manager;
		}
	}
}