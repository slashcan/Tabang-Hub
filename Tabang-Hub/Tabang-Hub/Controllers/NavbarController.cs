using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Tabang_Hub.Controllers
{
    public class NavbarController : BaseController
    {
        // GET: Navbar
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("../Page/Login");
        }
    }
}