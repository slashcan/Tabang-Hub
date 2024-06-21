using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Tabang_Hub.Utils;
using Tabang_Hub.Repository;
using System.Web.Security;

namespace Tabang_Hub.Controllers
{
    public class VolunteerController : BaseController
    {
        // GET: Volunteer
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult VolunteerProfile()
        {
            var getUserAccount = db.UserAccount.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

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
        public JsonResult EditBasicInfo(string phone, string street, string city, string province, string email)
        {
            try
            {
                var VolunteerUpdate = db.VolunteerInfo.Where(m => m.userId == UserId).FirstOrDefault();
                var UserUpdate = db.UserAccount.Where(m => m.userId == UserId).FirstOrDefault();

                VolunteerUpdate.street = street;
                VolunteerUpdate.phoneNum = phone;
                VolunteerUpdate.city = city;
                VolunteerUpdate.province = province;

                UserUpdate.email = email;

                db.SaveChanges();
                FormsAuthentication.SetAuthCookie(email, false);

                return Json(new { success = true, message = "Success !" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed !" });
            }
        }

        [HttpPost]
        public JsonResult EditAboutMe(string aboutMe, List<int?> skills)
        {
            try
            {
                var VolunteerUpdate = db.VolunteerInfo.Where(m => m.userId == UserId).FirstOrDefault();
                VolunteerUpdate.aboutMe = aboutMe;
                var getVolSkillCount = db.VolunteerSkill.Where(m => m.userId == UserId).Count();

                if (skills == null)
                {
                    var removeAllSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
                    foreach (var removeSkill in removeAllSkills)
                    {
                        db.VolunteerSkill.Remove(removeSkill);
                    }
                }
                else
                {

                    if (skills.Count < getVolSkillCount)
                    {
                        //Ang user ganahan tang tangon ang skill sa database
                        var skillToRemove = db.VolunteerSkill.Where(m => !skills.Contains(m.skillId)).ToList();

                        foreach (var removeSkill in skillToRemove)
                        {
                            db.VolunteerSkill.Remove(removeSkill);
                        }
                    }
                    if (skills != null && skills.Count > 0)
                    {

                        foreach (var skillId in skills)
                        {
                            var existSkill = db.VolunteerSkill.Where(m => m.userId == UserId && m.skillId == skillId).FirstOrDefault();
                            var getSkillName = db.Skills.Where(m => m.skillId == skillId).Select(m => m.skillName).FirstOrDefault();

                            if (existSkill == null)
                            {
                                var newVolSkill = new VolunteerSkill
                                {
                                    userId = UserId,
                                    skillId = skillId,
                                    skillName = getSkillName
                                };
                                db.VolunteerSkill.Add(newVolSkill);
                                Console.WriteLine($"Adding Skill ID: {skillId} for User ID: {UserId}");
                            }
                            else
                            {
                                Console.WriteLine($"Skill ID: {skillId} already exists for User ID: {UserId}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No skills provided.");
                    }
                }
                db.SaveChanges();
                return Json(new { success = true, message = "Success !" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error: " });
            }
        }
        [HttpPost]
        public ActionResult SaveInformation(VolunteerInfo model, List<string> volunteerSkill)
        {
            try
            {
                var volInfo = db.VolunteerInfo.Where(m => m.userId == UserId).FirstOrDefault();
                volInfo.fName = model.fName;
                volInfo.lName = model.lName;
                volInfo.bDay = model.bDay;
                volInfo.gender = model.gender;
                volInfo.street = model.street;
                volInfo.city = model.city;
                volInfo.province = model.province;
                volInfo.zipCode = model.zipCode;
                volInfo.phoneNum = model.phoneNum;

                foreach (var vSkill in volunteerSkill)
                {
                    var getSkill = _skills.GetAll().Where(m => m.skillName == vSkill).FirstOrDefault();
                    var skll = new VolunteerSkill
                    {
                        userId = UserId,
                        skillId = getSkill.skillId,
                        skillName = getSkill.skillName
                    };

                    _volunteerSkills.Create(skll);
                }

                db.SaveChanges();

                return Json(new { success = true, message = "Success" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error" });
            }
        }
        public ActionResult GeneralSkill()
        {
            var getEventImages = _eventImages.GetAll().ToList();
            var getSkills = _skills.GetAll().ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var getEvents = _listsOfEvent.GetAll().ToList();

            var getOrgEvents = _orgEvents.GetAll().FirstOrDefault();
            var getOrgInfo = db.OrgInfo.Where(m => m.userId == getOrgEvents.userId).ToList();

            var getSkillRequirements = _skillRequirement.GetAll().ToList();

            var indexModel = new Lists()
            {
                skills = getSkills,
                picture = getProfile,
                listOfEvents = getEvents,
                orgInfos = getOrgInfo,
                detailsSkillRequirement = getSkillRequirements,
                detailsEventImage = getEventImages
            };
            return View(indexModel);
        }
        public ActionResult EventDetails(int? eventId)
        {
            try
            {
                var checkEventID = _listsOfEvent.Get(eventId);
                if (checkEventID != null)
                {
                    var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
                    var getOrgInfo = db.OrgEvents.Where(m => m.eventId == eventId).FirstOrDefault();
                    var getInfo = _orgInfo.GetAll().Where(m => m.userId == getOrgInfo.userId).ToList();
                    var getSkillRequirmenet = _skillRequirement.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
                    var getOrgImages = _eventImages.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
                    var getEvent = _orgEvents.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
                    var getOrgOtherEvent = db.sp_OtherEvent(getOrgInfo.userId).ToList();
                    var getEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == getOrgInfo.eventId).ToList();
                    var getVolunteers = _volunteers.GetAll().Where(m => m.eventId == eventId).ToList();

                    var indexModel = new Lists()
                    {
                        picture = getProfile,
                        orgInfos = getInfo,
                        detailsSkillRequirement = getSkillRequirmenet,
                        detailsEventImage = getOrgImages,
                        orgEvents = getEvent,
                        orgOtherEvent = getOrgOtherEvent,
                        listOfEvents = getEvents,
                        volunteers = getVolunteers
                    };
                    return View(indexModel);
                }
                else
                {
                    return RedirectToAction("GeneralSkill"); //Error
                }
            }
            catch (Exception)
            {
                return RedirectToAction("GeneralSkill"); //Error
            }
        }
        [HttpPost]
        public JsonResult ApplyVolunteer(int eventId)
        {
            try
            {
                var checkVolunteer = _volunteers.GetAll().Where(m => m.userId == UserId).FirstOrDefault();

                var apply = new Volunteers()
                {
                    userId = UserId,
                    eventId = eventId
                };

                //var updateVolunteerNeeded = db.OrgEvents.Where(m => m.eventId == eventId).FirstOrDefault();
                
                if (checkVolunteer == null)
                {
                    //updateVolunteerNeeded.maxVolunteer = updateVolunteerNeeded.maxVolunteer - 1;
                    //db.SaveChanges();
                    _volunteers.Create(apply);

                    return Json(new { success = true, message = "Success !" });
                }
                else
                {
                    return Json(new { success = false, message = "Already apply !" });
                }
            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Error !" });
            }
        }
        public ActionResult DonationDetails(int eventId)
        {
            var getOrgInfo = db.OrgEvents.Where(m => m.eventId == eventId).FirstOrDefault();
            var listofUserDonated = _organizationManager.ListOfUserDonated(eventId);
            var getInfo = _orgInfo.GetAll().Where(m => m.userId == getOrgInfo.userId).ToList();
            var listofImage = _organizationManager.listOfEventImage(eventId);
            var getSkillRequirmenet = _skillRequirement.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
            var donation = _organizationManager.GetEventById(eventId);
            var orgInfo = _organizationManager.GetOrgInfoByUserId(donation.User_Id);
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

            var indexModel = new Utils.Lists()
            {
                OrgInfo = orgInfo,
                eventDetails = donation,
                detailsEventImage = listofImage,
                orgInfos = getInfo,
                detailsSkillRequirement = getSkillRequirmenet,
                picture = getProfile,
                listofUserDonated = listofUserDonated,
            };

            return View(indexModel);
        }
        [HttpPost]
        public ActionResult DonateNow(Lists donated)
        {
            donated.userDonated.userId = UserId;
            donated.userDonated.donatedAt = DateTime.Now;
            string errMsg = string.Empty;

            if (_volunteerManager.CreateDonation(donated.userDonated, ref errMsg) != ErrorCode.Success)
            {
                return Json(new { success = false, message = errMsg });
            }
            return Json(new { success = true });
        }

    }
}