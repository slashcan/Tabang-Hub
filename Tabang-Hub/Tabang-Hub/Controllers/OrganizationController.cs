using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
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
        #region Event Management
        public ActionResult EventsManagement()
        {
            var lists =_organizationManager.ListOfEvents();

            var indexModel = new Utils.Lists()
            {
                listOfEvents = lists,
            };

            return View(indexModel);
        }
        [HttpPost]
        public ActionResult CreateEvents(Utils.Lists events, string[] skills, HttpPostedFileBase[] images)
        {
            events.CreateEvents.userId = UserId;
            string errMsg = string.Empty;
            List<string> uploadedFiles = new List<string>();

            if (images != null && images.Length > 0)
            {
                foreach (var image in images)
                {
                    if (image != null && image.ContentLength > 0)
                    {
                        var inputFileName = Path.GetFileName(image.FileName);
                        var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                        if (!Directory.Exists(Server.MapPath("~/Content/IdPicture/")))
                            Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));


                        using (var srcImage = Image.FromStream(image.InputStream))
                        {
                            var newWidth = 400;
                            var newHeight = 300;
                            var resizedImage = new Bitmap(newWidth, newHeight);

                            using (var graphics = Graphics.FromImage(resizedImage))
                            {
                                graphics.CompositingQuality = CompositingQuality.HighQuality;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = SmoothingMode.HighQuality;
                                graphics.DrawImage(srcImage, 0, 0, newWidth, newHeight);
                            }

                            resizedImage.Save(serverSavePath, ImageFormat.Jpeg);
                        }

                        uploadedFiles.Add(inputFileName);
                    }
                }
            }
            if (_organizationManager.CreateEvents(events.CreateEvents, uploadedFiles, skills, ref errMsg) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, errMsg);
                return View();
            }
            return RedirectToAction("VolunteerManagement");
        }
        public ActionResult Details(int id) 
        { 
            var events = _organizationManager.GetEventById(id);
            var listofImage = _organizationManager.listOfEventImage(id);
            var listOfSkills = _organizationManager.listOfSkillRequirement(id);

            var indexModel = new Utils.Lists()
            {
                eventDetails = events,
                detailsEventImage = listofImage,
                detailsSkillRequirement = listOfSkills
            };

            if (events != null)
            { 
                return View(indexModel);
            }
            return RedirectToAction("EventsManagement");
        }
        #endregion

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