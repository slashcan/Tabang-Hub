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
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);

            var indexModel = new Lists()
            {
                OrgInfo = orgInfo,
                profilePic = profile,
            };
            return View(indexModel);
        }
        public ActionResult OrgProfile()
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);

            var indexModdel = new Lists()
            {
                OrgInfo = orgInfo,
                profilePic = profile,
            };
            return View(indexModdel);
        }
        [HttpPost]
        public ActionResult EditOrgProfile(Lists orgProfile, ProfilePicture profilePicture, HttpPostedFileBase profilePic)
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
        public ActionResult EventsList()
        {
            var lists = _organizationManager.ListOfEvents(UserId);
            var listOfSkill = _organizationManager.ListOfSkills();
            var listofUserDonated = _organizationManager.ListOfUserDonated(UserId);
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);

            var indexModel = new Lists()
            {
                listOfEvents = lists,
                listOfSkills = listOfSkill,
                listofUserDonated = listofUserDonated,
                OrgInfo = orgInfo,
                profilePic = profile,
            };

            return View(indexModel);
        }
        [HttpPost]
        public ActionResult CreateEvents(Lists events, Dictionary<string, int> skills, HttpPostedFileBase[] images)
        {
            // Sanitize skill names
            var sanitizedSkills = new Dictionary<string, int>();
            foreach (var skill in skills)
            {
                var sanitizedSkillName = skill.Key.Replace(" x", "").Trim();
                sanitizedSkills[sanitizedSkillName] = skill.Value;
            }

            events.CreateEvents.userId = UserId;
            string errMsg = string.Empty;
            List<string> uploadedFiles = new List<string>();

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
            if (events.CreateEvents.dateEnd <= events.CreateEvents.dateStart)
            {
                ModelState.AddModelError("CreateEvents.dateEnd", "End date and time cannot be before or the same as the start date and time.");
            }
            if (string.IsNullOrWhiteSpace(events.CreateEvents.location))
            {
                ModelState.AddModelError("CreateEvents.location", "Location is required.");
            }
            if (sanitizedSkills == null || sanitizedSkills.Count == 0)
            {
                ModelState.AddModelError("CreateEvents.skills", "At least one skill is required.");
            }
            if (images == null || images.Length == 0)
            {
                ModelState.AddModelError("CreateEvents.images", "At least one image is required.");
            }

            // Ensure the total volunteers from skills matches the maximum volunteers
            int totalVolunteers = 0;
            foreach (var skillCount in sanitizedSkills.Values)
            {
                totalVolunteers += skillCount;
            }
            events.CreateEvents.maxVolunteer = totalVolunteers;
            if (totalVolunteers != events.CreateEvents.maxVolunteer)
            {
                ModelState.AddModelError(string.Empty, "Total volunteers assigned to skills must equal the maximum number of volunteers.");
                return RedirectToAction("EventsList");
            }

            // Image processing
            if (images != null && images.Length > 0)
            {
                var imagePath = Server.MapPath("~/Images/Events");
                Directory.CreateDirectory(imagePath); // Create directory if it doesn't exist

                foreach (var image in images)
                {
                    if (image != null && image.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(image.FileName);
                        var filePath = Path.Combine(imagePath, fileName);
                        image.SaveAs(filePath);
                        uploadedFiles.Add(fileName);
                    }
                }
            }

            // Store the event and associated skill requirements
            if (_organizationManager.CreateEvents(events.CreateEvents, uploadedFiles, sanitizedSkills, ref errMsg) != ErrorCode.Success)
            {
                ModelState.AddModelError(string.Empty, errMsg);
                return RedirectToAction("EventsList");
            }

            TempData["Success"] = true;
            return RedirectToAction("EventsList");
        }
        public ActionResult Details(int id)
        {
            var events = _organizationManager.GetEventById(id);
            var listofImage = _organizationManager.listOfEventImage(id);
            var listOfSkills = _organizationManager.listOfSkillRequirement(id);
            var listofUserDonated = _organizationManager.ListOfUserDonated(id);
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var listOfEventVolunteers = _organizationManager.ListOfEventVolunteers(id);
            var volunteerSkills = _organizationManager.ListOfEventVolunteerSkills();
            var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);
            var listOfSkill = _organizationManager.ListOfSkills();

            var indexModel = new Lists()
            {
                OrgInfo = orgInfo,
                eventDetails = events,
                listOfSkills = listOfSkill,
                detailsEventImage = listofImage,
                detailsSkillRequirement = listOfSkills,
                listOfEventVolunteers = listOfEventVolunteers,
                volunteersSkills = volunteerSkills,
                listofUserDonated = listofUserDonated,
                profilePic = profile,
            };

            if (events != null)
            {
                return View(indexModel);
            }
            return RedirectToAction("EventsManagement");
        }
        [HttpPost]
        public ActionResult EditEvent(Lists events, String[] skills, HttpPostedFileBase[] images, int eventId)
        {
            string errMsg = string.Empty;
            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };
            List<string> uploadedFiles = new List<string>();

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
                            return RedirectToAction("Details", new { id = eventId });
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
                            return RedirectToAction("Details", new { id = eventId });
                        }
                    }
                }
            }

            if (_organizationManager.EditEvent(events.CreateEvents, skills, uploadedFiles, eventId, ref errMsg) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, errMsg);
                return RedirectToAction("Details", new { id = eventId });
            }

            return RedirectToAction("Details", new { id = eventId });
        }

        [HttpPost]
        public ActionResult Delete(int eventId)
        {
            var deleteEvent = _organizationManager.DeleteEvent(eventId);

            if (deleteEvent != ErrorCode.Success)
            {
                // You may want to set a TempData or ViewBag message to inform the user of the error
                TempData["ErrorMessage"] = "There was an error deleting the event. Please try again.";
                return RedirectToAction("EventsList");
            }

            // You may want to set a TempData or ViewBag message to inform the user of the success
            TempData["SuccessMessage"] = "Event deleted successfully.";
            return RedirectToAction("EventsList");
        }
        [HttpPost]
        public ActionResult ConfirmApplicants(int id, int eventId)
        {
            string errMsg = string.Empty;

            if (_organizationManager.ConfirmApplicants(id, eventId, ref errMsg) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, errMsg);
                return RedirectToAction("EventsList");
            }
            return RedirectToAction("EventsList");
        }
        public ActionResult VolunteerDetails(int userId)
        {
            var getUserAccount = _organizationManager.GetUserByUserId(userId);
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == userId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == userId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == userId).ToList();
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);

            var listModel = new Lists()
            {
                OrgInfo = orgInfo,
                userAccount = getUserAccount,
                volunteersInfo = getVolunteerInfo,
                volunteersSkills = getVolunteerSkills,
                picture = getProfile,
                profilePic = profile,
            };

            return View(listModel);
        }
        public ActionResult Reports()
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);

            var indexModdel = new Lists()
            {
                OrgInfo = orgInfo,
                profilePic = profile,
            };
            return View(indexModdel);
        }
        public ActionResult History()
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);
            var eventHistory = _organizationManager.GetEventHistoryByUserId(UserId);

            var indexModdel = new Lists()
            {
                OrgInfo = orgInfo,
                profilePic = profile,
                orgEventHistory = eventHistory,
            };
            return View(indexModdel);
        }
        [HttpPost]
        public JsonResult TransferToHistory()
        {
            var userId = UserId;
            string errMsg = string.Empty;

            var result = _organizationManager.TransferToHistory(userId, ref errMsg);
            if (result != ErrorCode.Success)
            {
                return Json(new { success = false, message = errMsg });
            }

            return Json(new { success = true, message = "Events successfully transferred to history." });
        }
    }
}