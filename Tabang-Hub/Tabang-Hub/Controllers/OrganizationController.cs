using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tabang_Hub.Controllers
{
    public class OrganizationController : Controller
    {
        // GET: Organization
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult VolunteerManagement()
        {
            return View();
        }
        public ActionResult EventsManagement()
        {
            return View();
        }
        public ActionResult DonationsManagement()
        {
            return View();
        }
        public ActionResult ReportsManagement()
        {
            return View();
        }
    }
}