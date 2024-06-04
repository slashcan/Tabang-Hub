using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Repository;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Controllers
{
    public class OrganizationController : BaseController
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
        [HttpPost]
        public ActionResult CreateEvents(Utils.Lists events, OrgEventImage orgEvenImage, string[] skills, HttpPostedFileBase image)
        {
            string errMsg = string.Empty;

            if (image != null && image.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(image.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/UploadedFiles/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                image.SaveAs(serverSavePath);

                orgEvenImage.eventImage = inputFileName;
            }

            if (_organizationManager.CreateEvents(events.CreateEvents, orgEvenImage, skills, ref ErrorMessage) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);
                return View();
            }
            return RedirectToAction("VolunteerManagement");                  
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