using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tabang_Hub.Controllers
{
    public class PageController : BaseController
    {
        // GET: Page
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(UserAccount u)
        {
            u.status = 0;
            u.roleId = 1;

            _userAcc.Create(u);
            TempData["Msg"] = $"User {u.email} added!";

            return View();
        }
    }
}