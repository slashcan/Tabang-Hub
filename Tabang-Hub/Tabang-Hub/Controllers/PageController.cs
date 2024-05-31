using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Controllers
{
    public class PageController : BaseController
    {
        // GET: Page
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(UserAccount u, String ConfirmPass)
        {
            u.status = 0;
            u.roleId = 1;

            try
            {
                if (!u.password.Equals(ConfirmPass))
                {
                    ModelState.AddModelError(String.Empty, "Password not match");
                    return View(u);
                }

                if (_userManager.Register(u, ref ErrorMessage) != ErrorCode.Success)
                {
                    ModelState.AddModelError(String.Empty, ErrorMessage);

                    return View(u);
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = $"Error! " + ex.Message;
                return RedirectToAction("Register");
            }

            return RedirectToAction("Login");
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(String email, String password)
        {
            if (_userManager.Login(email, password, ref ErrorMessage) == ErrorCode.Success)
            {
                var user = _userManager.GetUserByEmail(email);
            }

            FormsAuthentication.SetAuthCookie(email, false);

            return RedirectToAction("Index");
        }
    }
}