using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web;
using System.Web.Mvc;

namespace Carrotware.CMS.Security {

	public enum ManageMessageId {
		AddPhoneSuccess,
		ChangePasswordSuccess,
		SetTwoFactorSuccess,
		SetPasswordSuccess,
		RemoveLoginSuccess,
		RemovePhoneSuccess,
		Error
	}

	public class ManageSecurity {
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;
		private IAuthenticationManager _authnManager;
		private ApplicationRoleManager _roleManager;

		private Controller _controller;

		public ManageSecurity(Controller controller) {
			_controller = controller;
		}

		public ManageSecurity(Controller controller, ApplicationUserManager userManager,
			ApplicationSignInManager signInManager, IAuthenticationManager authManager) {
			UserManager = userManager;
			SignInManager = signInManager;
			AuthenticationManager = authManager;
		}

		public ApplicationSignInManager SignInManager {
			get {
				return _signInManager ?? _controller.HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set {
				_signInManager = value;
			}
		}

		public ApplicationUserManager UserManager {
			get {
				return _userManager ?? _controller.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set {
				_userManager = value;
			}
		}

		public IAuthenticationManager AuthenticationManager {
			get {
				return _authnManager ?? _controller.HttpContext.GetOwinContext().Authentication;
			}
			private set {
				_authnManager = value;
			}
		}

		public ApplicationRoleManager RoleManager {
			get {
				return _roleManager ?? _controller.HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
			}
			private set { _roleManager = value; }
		}
	}
}