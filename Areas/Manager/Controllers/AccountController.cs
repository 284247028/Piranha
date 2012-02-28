﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using Piranha.Models;

namespace Piranha.Areas.Manager.Controllers
{
	/// <summary>
	/// Login controller for the manager interface.
	/// </summary>
    public class AccountController : Controller
    {
		/// <summary>
		/// Default action
		/// </summary>
        public ActionResult Index()
        {
			// Check for existing installation.
			try {
				if (Data.Database.InstalledVersion < Data.Database.CurrentVersion)
					return RedirectToAction("Update", "Install") ;
	            return View("Index") ;
			} catch {}
			return RedirectToAction("Index", "Install") ;
        }

		/// <summary>
		/// Logs in the provided user.
		/// </summary>
		/// <param name="m">The model</param>
		[HttpPost()]
		public ActionResult Login(LoginModel m) {
			// Authenticate the user
			if (ModelState.IsValid) {
				SysUser user = SysUser.Authenticate(m.Login, m.Password) ;
				if (user != null) {
					FormsAuthentication.SetAuthCookie(user.Id.ToString(), m.RememberMe) ;
					HttpContext.Session[PiranhaApp.USER] = user ;

					// Redirect after logon
					return RedirectToAction("Index", "Page") ;
				} else {
					ViewBag.Message = @Piranha.Resources.Account.MessageLoginFailed ;
				}
			} else {
				ViewBag.Message = @Piranha.Resources.Account.MessageLoginEmptyFields ;
			}
			return Index() ;
		}

		/// <summary>
		/// Logs out the current user.
		/// </summary>
		public ActionResult Logout() {
			FormsAuthentication.SignOut() ;
			Session.Clear() ;
			Session.Abandon() ;

			return RedirectToAction("Index") ;
		}
    }
}
