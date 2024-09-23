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
    public class AdminController : BaseController
    {
        // GET: Admin
        public ActionResult Index()
        {         
            return View();
        }
        public ActionResult VolunteerAccounts()
        {
            
            var volunteerAccount = _adminManager.GetVolunteerAccounts();

            var indexModel = new Lists()
            {
                volunteerAccounts = volunteerAccount,
            };
            return View(indexModel);
        }

        public ActionResult OrganizationAccounts()
        {

            var organizationAccount = _adminManager.GetOrganizationAccounts();

            var indexModel = new Lists()
            {
                organizationAccounts = organizationAccount,
            };
            return View(indexModel);
        }

        public ActionResult UserDetails(int userId)
        {
            var getUserAccount = db.UserAccount.Where(m => m.userId == userId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == userId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == userId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == userId).ToList();

            var getUniqueSkill = db.sp_GetSkills(UserId).ToList();

            var listModel = new Lists()
            {
                userAccounts = getUserAccount,
                volunteersInfo = getVolunteerInfo,
                volunteersSkills = getVolunteerSkills,
                uniqueSkill = getUniqueSkill,
                picture = getProfile
            };

            return View(listModel);
        }
        [HttpPost]
        public ActionResult Register(UserAccount u, VolunteerInfo v, UserRoles r, String ConfirmPass)
        {
            u.status = 0;
            u.roleId = 1;

            try
            {
                if (!u.password.Equals(ConfirmPass))
                {
                    ModelState.AddModelError(String.Empty, "Password not match");
                    return RedirectToAction("VolunteerAccounts");
                }

                if (_userManager.Register(u, v, r, ref ErrorMessage) != ErrorCode.Success)
                {
                    ModelState.AddModelError(String.Empty, ErrorMessage);

                    return RedirectToAction("VolunteerAccounts");
                }              
            }
            catch (Exception ex)
            {
                TempData["msg"] = $"Error! " + ex.Message;
                return RedirectToAction("VolunteerAccounts");
            }

            return RedirectToAction("VolunteerAccounts");
        }
        [HttpPost]
        public ActionResult RegisterOrg(UserAccount u, OrgInfo o, OrgValidation ov, UserRoles r, HttpPostedFileBase picture1, HttpPostedFileBase picture2)
        {
            if (picture1 != null && picture1.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(picture1.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/UploadedFiles/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                picture1.SaveAs(serverSavePath);

                ov.idPicture1 = inputFileName;
            }
            if (picture2 != null && picture2.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(picture2.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/UploadedFiles/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                picture2.SaveAs(serverSavePath);

                ov.idPicture2 = inputFileName;
            }
            if (_userManager.OrgRegister(u, o, ov, r, ref ErrorMessage) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);
            }
            return RedirectToAction("OrganizationAccounts");
        }
        [HttpPost]
        public ActionResult DeleteUser(int userId)
        {
            try
            {
                var user = _adminManager.DeleteUser(userId);
                if (user != ErrorCode.Success )
                {
                    return Json(new { success = false, message = "User not found" });

                }
                else
                {
                    
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult DeleteOrg(int userId)
        {
            try
            {
                var user = _adminManager.DeleteOrganization(userId);
                if (user != ErrorCode.Success)
                {
                    return Json(new { success = false, message = "User not found" });

                }
                else
                {

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult ManageSkill()
        { 
            var skills = _adminManager.GetSkills();

            var indexModel = new Lists()
            { 
                allSkill = skills,
            };
            return View(indexModel);
        }
        [HttpPost]
        public ActionResult AddSkills(Skills skill, HttpPostedFileBase skillImage)
        {
            if (skillImage != null && skillImage.ContentLength > 0)
            {
                // Define the directory path
                var directoryPath = Server.MapPath("~/Content/SkillImages/");

                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(directoryPath);
                }

                // Get the file name and full path
                var fileName = Path.GetFileName(skillImage.FileName);
                var path = Path.Combine(directoryPath, fileName);

                // Save the file
                skillImage.SaveAs(path);

                // Set the image file name in the skill object
                skill.skillImage = fileName;

                string errMsg = string.Empty;
                if (_adminManager.AddSkills(skill, ref errMsg) == ErrorCode.Success)
                {
                    TempData["SuccessMessage"] = "Skill added successfully!";
                    return RedirectToAction("ManageSkill");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errMsg);
                }
            }

            TempData["ErrorMessage"] = "An error occurred while adding the skill.";
            return View("ManageSkill");
        }

        // Action to handle the deletion of a skill
        [HttpPost]
        public JsonResult DeleteSkill(int skillId)
        {
            string errMsg = string.Empty;
            if (_adminManager.DeleteSkill(skillId) == ErrorCode.Success)
            {
                return Json(new { success = true, message = "Skill deleted successfully." });
            }

            return Json(new { success = false, message = errMsg });
        }
        [HttpPost]
        public ActionResult Deactivate(int userId)
        {
            string errMsg = string.Empty;
            if (_adminManager.DeactivateAccount(userId, ref errMsg) != ErrorCode.Success)
            {
                return Json(new { success = false, message = errMsg });
            }
            return Json(new { success = true });
        }
        public ActionResult Reports()
        {

            var userId = 35;
            var orgInfo = _organizationManager.GetOrgInfoByUserId(userId);
            var profile = _organizationManager.GetProfileByProfileId(userId);
            var events = _organizationManager.ListOfEvents(userId);
            var totalDonation = _organizationManager.GetTotalDonationByUserId(userId);
            var totalVolunteer = _organizationManager.GetTotalVolunteerByUserId(userId);
            var eventSummary = _organizationManager.GetEventsByUserId(userId);
            var recentEvents = _organizationManager.GetRecentOngoingEventsByUserId(userId);
            var totalSkills = _organizationManager.GetAllVolunteerSkills(userId);
            var userDonated = _organizationManager.GetRecentUserDonationsByUserId(userId);

            var indexModdel = new Lists()
            {
                OrgInfo = orgInfo,
                listOfEvents = events,
                totalDonation = totalDonation,
                totalVolunteer = totalVolunteer,
                eventSummary = eventSummary,
                recentEvents = recentEvents,
                totalSkills = totalSkills,
                recentDonators = userDonated,
                //profilePic = profile,
            };
            return View(indexModdel);
        }

    }
}