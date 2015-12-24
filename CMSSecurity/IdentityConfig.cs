using Carrotware.CMS.Security.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
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
			Config();
		}

		public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) {
			var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<SecurityDbContext>()));

			//manager.Config();

			//var dataProtectionProvider = options.DataProtectionProvider;
			//if (dataProtectionProvider != null) {
			//	manager.UserTokenProvider =
			//		new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
			//}

			return manager;
		}

		public void Config() {
			CarrotSecurityConfig config = CarrotSecurityConfig.GetConfig();

			// Configure validation logic for usernames
			this.UserValidator = new UserValidator<ApplicationUser>(this) {
				AllowOnlyAlphanumericUserNames = config.UserValidator.AllowOnlyAlphanumericUserNames,
				RequireUniqueEmail = config.UserValidator.RequireUniqueEmail
			};

			// Configure validation logic for passwords
			this.PasswordValidator = new PasswordValidator {
				RequiredLength = config.PasswordValidator.RequiredLength,
				RequireNonLetterOrDigit = config.PasswordValidator.RequireNonLetterOrDigit,
				RequireDigit = config.PasswordValidator.RequireDigit,
				RequireLowercase = config.PasswordValidator.RequireLowercase,
				RequireUppercase = config.PasswordValidator.RequireUppercase,
			};

			// Configure user lockout defaults
			this.UserLockoutEnabledByDefault = config.AdditionalSettings.UserLockoutEnabledByDefault;
			this.MaxFailedAccessAttemptsBeforeLockout = config.AdditionalSettings.MaxFailedAccessAttemptsBeforeLockout;
			this.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(config.AdditionalSettings.DefaultAccountLockoutTimeSpan);

			// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
			// You can write your own provider and plug it in here.
			this.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser> {
				MessageFormat = "Your security code is {0}"
			});

			this.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser> {
				Subject = "Security Code",
				BodyFormat = "Your security code is {0}"
			});

			this.EmailService = new EmailService();
			this.SmsService = new SmsService();

			var provider = new DpapiDataProtectionProvider(config.AdditionalSettings.DataProtectionProviderAppName);

			if (provider != null) {
				this.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("UserToken")) {
					TokenLifespan = TimeSpan.FromDays(7)
				};
			}
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