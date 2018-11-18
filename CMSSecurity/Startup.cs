using Carrotware.CMS.Security.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(Carrotware.CMS.Security.Startup))]

namespace Carrotware.CMS.Security {

	public class Startup {

		public void Configuration(IAppBuilder app) {
			ConfigureAuth(app);
		}

		public void ConfigureAuth(IAppBuilder app) {
			// Configure the db context, user manager and signin manager to use a single instance per request
			app.CreatePerOwinContext(SecurityDbContext.Create);

			app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
			app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
			app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

			// Enable the application to use a cookie to store information for the signed in user
			// and to use a cookie to temporarily store information about a user logging in with a third party login provider
			// Configure the sign in cookie

			CarrotSecurityConfig config = CarrotSecurityConfig.GetConfig();

			bool setCookieExpireTimeSpan = config.AdditionalSettings.SetCookieExpireTimeSpan;
			string loginPath = config.AdditionalSettings.LoginPath;

			double expireTimeSpan = config.AdditionalSettings.ExpireTimeSpan;
			double validateInterval = config.AdditionalSettings.ValidateInterval;

			if (expireTimeSpan < 5) {
				expireTimeSpan = 5;
			}
			if (validateInterval < 5) {
				validateInterval = 5;
			}

			//because otherwise you'll get constantly logged out
			if (expireTimeSpan < validateInterval) {
				expireTimeSpan = validateInterval + 1;
			}

			double cookieLife = (setCookieExpireTimeSpan ? expireTimeSpan : validateInterval) + 2;

			app.UseCookieAuthentication(new CookieAuthenticationOptions {
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
				LoginPath = new PathString(loginPath),
				Provider = new CookieAuthenticationProvider {
					// Enables the application to validate the security stamp when the user logs in.
					// This is a security feature which is used when you change a password or add an external login to your account.
					OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
						validateInterval: TimeSpan.FromMinutes(validateInterval),
						regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),
					OnResponseSignIn = (context) => {
						context.Properties.IsPersistent = true;
						context.Properties.AllowRefresh = true;
						context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(cookieLife);
					}
				},
				CookieHttpOnly = true,
				ExpireTimeSpan = TimeSpan.FromMinutes(expireTimeSpan),
				SlidingExpiration = true
			});
			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

			// Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
			app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

			// Enables the application to remember the second login verification factor such as phone or email.
			// Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
			// This is similar to the RememberMe option when you log in.
			app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
		}
	}
}