﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Tabang_Hub.ListModel;
using Tabang_Hub.Repository;
using Tabang_Hub.Utils;

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
            var getUserAccount = db.UserAccounts.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfoes.Where(m => m.userId == UserId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();

            var getUniqueSkill = db.sp_GetSkills(UserId).ToList();

            var listModel = new IndexModel()
            {
                userAccounts = getUserAccount,
                volunteersInfo = getVolunteerInfo,
                volunteersSkill = getVolunteerSkills,
                uniqueSkill = getUniqueSkill
            };

            return View(listModel);
        }

        [HttpPost]
        public JsonResult EditBasicInfo(string phone, string street, string city, string province, string email)
        {
            try
            {
                var VolunteerUpdate = db.VolunteerInfoes.Where(m => m.userId == UserId).FirstOrDefault();
                var UserUpdate = db.UserAccounts.Where(m => m.userId == UserId).FirstOrDefault();

                VolunteerUpdate.street = street;
                VolunteerUpdate.phoneNum = phone;
                VolunteerUpdate.city = city;
                VolunteerUpdate.province = province;

                UserUpdate.email = email;

                db.SaveChanges();

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
                var VolunteerUpdate = db.VolunteerInfoes.Where(m => m.userId == UserId).FirstOrDefault();
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
                            var getSkillName = db.Skills.Where(m => m.skillId == skillId).Select(m => m.SkillName).FirstOrDefault();

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
                var volInfo = db.VolunteerInfoes.Where(m => m.userId == UserId).FirstOrDefault();
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
                    var getSkill = _skills.GetAll().Where(m => m.SkillName == vSkill).FirstOrDefault();
                    var skll = new VolunteerSkill
                    {
                        userId = UserId,
                        skillId = getSkill.skillId,
                        skillName = getSkill.SkillName
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
    }
}