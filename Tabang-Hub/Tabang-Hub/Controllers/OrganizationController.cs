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
        public ActionResult OrgProfile()
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);

            var indexModdel = new Utils.Lists()
            {
                OrgInfo = orgInfo,
            };
            return View(indexModdel);
        }
        [HttpPost]
        public ActionResult EditOrgProfile(Utils.Lists orgProfile, ProfilePicture profilePicture, HttpPostedFileBase profilePic)
        {
            string errMsg = string.Empty;
            profilePicture.userId = UserId;

            // Assuming you have a method to get the current user ID

            if (profilePic != null && profilePic.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(profilePic.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/Content/IdPicture/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                profilePic.SaveAs(serverSavePath);

                profilePicture.profilePath = "~/Content/IdPicture/" + inputFileName;
            }

            if (_organizationManager.EditOrgInfo(orgProfile.OrgInfo, profilePicture, UserId, ref errMsg) != ErrorCode.Success)
            {
                ModelState.AddModelError(string.Empty, errMsg);
                return View("OrgProfile", orgProfile); // Returning the view with the model to display validation errors
            }         

            return RedirectToAction("OrgProfile");
        }

        public ActionResult VolunteerManagement()
        {
            return View();
        }
        #region Event Management
        public ActionResult EventsManagement()
        {
            var lists =_organizationManager.ListOfEvents(UserId, 1);

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
            events.CreateEvents.eventType = 1;
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
            return RedirectToAction("EventsManagement");
        }
        public ActionResult Details(int id) 
        { 
            var events = _organizationManager.GetEventById(id);
            var listofImage = _organizationManager.listOfEventImage(id);
            var listOfSkills = _organizationManager.listOfSkillRequirement(id);
            var orgInfo  = _organizationManager.GetOrgInfoByUserId(UserId);


            var indexModel = new Utils.Lists()
            {
                OrgInfo = orgInfo,
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

        #region Organization Management
        public ActionResult DonationsManagement()
        {
            var lists = _organizationManager.ListOfEvents(UserId, 2);

            var indexModel = new Utils.Lists()
            {
                listOfEvents = lists,
            };

            return View(indexModel);
        }
        [HttpPost]
        public ActionResult CreateDonations(Utils.Lists events, HttpPostedFileBase[] images)
        {
            events.CreateEvents.userId = UserId;
            events.CreateEvents.eventType = 2;
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
            String[] skills = null;
            if (_organizationManager.CreateEvents(events.CreateEvents, uploadedFiles,skills, ref errMsg) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, errMsg);
                return View();
            }
            return RedirectToAction("DonationsManagement");
        }
        #endregion
        public ActionResult ReportsManagement()
        {
            return View();
        }
    }
}