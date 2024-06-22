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
            var lists = _organizationManager.ListOfEvents(UserId, 1);

            var indexModel = new Lists()
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

            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };

            // Server-side validation
            if (string.IsNullOrWhiteSpace(events.CreateEvents.eventTitle))
            {
                ModelState.AddModelError("CreateEvents.eventTitle", "Event Title is required.");
            }
            if (string.IsNullOrWhiteSpace(events.CreateEvents.eventDescription))
            {
                ModelState.AddModelError("CreateEvents.eventDescription", "Event Description is required.");
            }
            if (events.CreateEvents.maxVolunteer <= 0)
            {
                ModelState.AddModelError("CreateEvents.maxVolunteer", "Maximum Volunteers must be greater than 0.");
            }
            if (events.CreateEvents.dateStart == null || events.CreateEvents.dateEnd == null)
            {
                ModelState.AddModelError("CreateEvents.dateStart", "Start Date and End Date are required.");
            }
            if (events.CreateEvents.dateStart < DateTime.Now)
            {
                ModelState.AddModelError("CreateEvents.dateStart", "Start date and time cannot be before the current date and time.");
            }
            if (events.CreateEvents.dateEnd < events.CreateEvents.dateStart)
            {
                ModelState.AddModelError("CreateEvents.dateEnd", "End date and time cannot be before the start date and time.");
            }
            if (string.IsNullOrWhiteSpace(events.CreateEvents.location))
            {
                ModelState.AddModelError("CreateEvents.location", "Location is required.");
            }
            if (skills == null || skills.Length == 0)
            {
                ModelState.AddModelError("CreateEvents.skills", "At least one skill is required.");
            }
            if (images == null || images.Length == 0)
            {
                ModelState.AddModelError("CreateEvents.images", "At least one image is required.");
            }

            // Check if there are validation errors
            if (!ModelState.IsValid)
            {
                return RedirectToAction("EventsManagement");
            }

            if (images != null && images.Length > 0)
            {
                foreach (var image in images)
                {
                    if (image != null && image.ContentLength > 0)
                    {
                        var extension = Path.GetExtension(image.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError(String.Empty, "Invalid file type. Only JPG, JPEG, PNG, and GIF files are allowed.");
                            return RedirectToAction("EventsManagement");
                        }

                        var inputFileName = Path.GetFileName(image.FileName);
                        var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                        if (!Directory.Exists(Server.MapPath("~/Content/IdPicture/")))
                            Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                        try
                        {
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
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(String.Empty, $"Error processing file {inputFileName}: {ex.Message}");
                            return RedirectToAction("EventsManagement");
                        }
                    }
                }
            }

            if (_organizationManager.CreateEvents(events.CreateEvents, uploadedFiles, skills, ref errMsg) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, errMsg);
                return RedirectToAction("EventsManagement");
            }

            return RedirectToAction("EventsManagement");
        }
        public ActionResult Details(int id)
        {
            var events = _organizationManager.GetEventById(id);
            var listofImage = _organizationManager.listOfEventImage(id);
            var listOfSkills = _organizationManager.listOfSkillRequirement(id);
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var listOfEventVolunteers = _organizationManager.ListOfEventVolunteers(id);
            var volunteerSkills = _organizationManager.ListOfEventVolunteerSkills();

            var indexModel = new Lists()
            {
                OrgInfo = orgInfo,
                eventDetails = events,
                detailsEventImage = listofImage,
                detailsSkillRequirement = listOfSkills,
                listOfEventVolunteers = listOfEventVolunteers,
                volunteersSkills = volunteerSkills,
            };

            if (events != null)
            {
                return View(indexModel);
            }
            return RedirectToAction("EventsManagement");
        }
        [HttpPost]
        public ActionResult Delete(int eventId)
        {
            var deleteEvent = _organizationManager.DeleteEvent(eventId);

            if (deleteEvent != ErrorCode.Success)
            {
                // You may want to set a TempData or ViewBag message to inform the user of the error
                TempData["ErrorMessage"] = "There was an error deleting the event. Please try again.";
                return RedirectToAction("EventsManagement");
            }

            // You may want to set a TempData or ViewBag message to inform the user of the success
            TempData["SuccessMessage"] = "Event deleted successfully.";
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
            if (_organizationManager.CreateEvents(events.CreateEvents, uploadedFiles, skills, ref errMsg) != ErrorCode.Success)
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