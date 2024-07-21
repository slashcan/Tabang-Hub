using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tabang_Hub.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult VolunteerAccounts()
        {
            return View();
        }

        public ActionResult OrganizationAccounts()
        {
            return View();
        }
    }
}