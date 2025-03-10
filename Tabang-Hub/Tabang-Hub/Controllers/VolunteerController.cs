﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Tabang_Hub.Utils;
using Tabang_Hub.Repository;
using System.Web.Security;
using System.Web.Management;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Tabang_Hub.Hubs;
using Microsoft.AspNet.SignalR;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace Tabang_Hub.Controllers
{
    public class VolunteerController : BaseController
    {
        // GET: Volunteer
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public async Task<ActionResult> VolunteerProfile()
        {
            var getUserAccount = db.UserAccount.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

            var recommendedEvents = await _volunteerManager.RunRecommendation(UserId);

            var filteredEvent = new List<vw_ListOfEvent>();
            foreach (var recommendedEvent in recommendedEvents)
            {
                var matchedEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == recommendedEvent.EventID).ToList();
                filteredEvent.AddRange(matchedEvents);
            }

            var getUniqueSkill = db.sp_GetSkills(UserId).ToList();
            if (getProfile.Count() <= 0)
            {
                var defaultPicture = new ProfilePicture
                {
                    userId = UserId,
                    profilePath = "default.jpg"
                };
                _profilePic.Create(defaultPicture);

                getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            }
            var listModel = new Lists()
            {
                userAccounts = getUserAccount,
                volunteersInfo = getVolunteerInfo,
                volunteersSkills = getVolunteerSkills,
                uniqueSkill = getUniqueSkill,
                picture = getProfile,
                skills = _skills.GetAll().ToList(),
                volunteersHistories = _volunteerManager.GetVolunteersHistoryByUserId(UserId),
                rating = db.Rating.Where(m => m.userId == UserId).ToList(),
                orgEventHistory = db.OrgEvents.Where(m => m.userId == UserId && m.status == 2).ToList(),
                listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
                detailsEventImage = _eventImages.GetAll().ToList()
            };

            return View(listModel);
        }

        [HttpPost]
        public JsonResult EditBasicInfo(string phone, string street, string city, string province, string availability, HttpPostedFileBase profilePic)
        {
            try
            {
                var VolunteerUpdate = db.VolunteerInfo.Where(m => m.userId == UserId).FirstOrDefault();
                var UserUpdate = db.UserAccount.Where(m => m.userId == UserId).FirstOrDefault();

                VolunteerUpdate.street = street;
                VolunteerUpdate.phoneNum = phone;
                VolunteerUpdate.city = city;
                VolunteerUpdate.province = province;
                VolunteerUpdate.availability = availability;
                //UserUpdate.email = email;

                if (profilePic != null)
                {
                    // Save the profile picture to the server
                    string fileName = Path.GetFileName(profilePic.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/UserProfile"), fileName);
                    profilePic.SaveAs(filePath);

                    var vProfile = db.ProfilePicture.Where(m => m.userId == UserId).FirstOrDefault();

                    // Update the profile picture path in the database
                    vProfile.profilePath = fileName;
                }

                db.SaveChanges();
                //FormsAuthentication.SetAuthCookie(email, false);

                return Json(new { success = true, message = "Success !" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed !" });
            }
        }
        [HttpGet]
        public JsonResult GetUnreadNotifications()
        {
            int userId = UserId;

            var notifications = db.Notification
                .Where(n => n.userId == userId && n.status == 0)
                .OrderByDescending(n => n.createdAt) // Assuming 'CreatedAt' is the date field
                .Select(n => new { n.type, n.content })
                .ToList();

            return Json(notifications, JsonRequestBehavior.AllowGet);
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
                        // Get the list of event IDs the user has joined
                        var joinedEvents = db.Volunteers
                            .Where(m => m.userId == UserId)
                            .Select(m => m.eventId)
                            .ToList();

                        // Get the list of required skills for these events
                        var requiredSkills = db.OrgSkillRequirement
                            .Where(m => joinedEvents.Contains(m.eventId))
                            .Select(m => m.skillId)
                            .ToList();

                        // Find skills the user wants to remove
                        var skillToRemove = db.VolunteerSkill
                            .Where(m => !skills.Contains(m.skillId) && m.userId == UserId)
                            .ToList();

                        foreach (var removeSkill in skillToRemove)
                        {
                            if (requiredSkills.Contains(removeSkill.skillId))
                            {
                                // Prevent removal if it's a required skill
                                return Json(new { success = false, message = $"Skill {removeSkill.skillId} is required for a joined event and cannot be removed." });
                            }
                            db.VolunteerSkill.Remove(removeSkill);
                        }
                    }

                    if (skills != null)
                    {

                        foreach (var skillId in skills)
                        {
                            var existSkill = db.VolunteerSkill.Where(m => m.userId == UserId && m.skillId == skillId).FirstOrDefault();
                            var getSkillName = db.Skills.Where(m => m.skillId == skillId).Select(m => m.skillName).FirstOrDefault();

                            if (existSkill == null)
                            {
                                var skillToRemove = db.VolunteerSkill.Where(m => !skills.Contains(m.skillId) && m.userId == UserId).ToList();

                                foreach (var removeSkill in skillToRemove)
                                {
                                    db.VolunteerSkill.Remove(removeSkill);
                                }

                                var newVolSkill = new VolunteerSkill
                                {
                                    userId = UserId,
                                    skillId = skillId
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
                volInfo.availability = model.availability;

                foreach (var vSkill in volunteerSkill)
                {
                    var getSkill = _skills.GetAll().Where(m => m.skillName == vSkill).FirstOrDefault();
                    var skll = new VolunteerSkill
                    {
                        userId = UserId,
                        skillId = getSkill.skillId
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
        [HttpPost]
        public JsonResult CheckPhoneNumber(string phoneNumber)
        {
            //var existingUser = db.VolunteerInfo.FirstOrDefault(m => m.phoneNum == phoneNumber);
            var logUser = db.VolunteerInfo.Where(m => m.userId == UserId).FirstOrDefault();
            var existingUser = db.VolunteerInfo.FirstOrDefault(m => m.phoneNum == phoneNumber);

            // If both user and phone number do not exist
            if (logUser == null && existingUser == null)
            {
                return Json(new { success = true, message = "Phone number is valid." });
            }

            // If the user exists and the phone number is the same as their current one
            if (logUser != null && logUser.phoneNum == phoneNumber)
            {
                return Json(new { success = true, message = "Phone number is valid." });
            }

            // If the user exists but the phone number is used by another account
            if (logUser != null && existingUser != null && logUser.phoneNum != phoneNumber)
            {
                return Json(new { success = false, message = "This number is linked to another account. Please try a different one." });
            }

            // If the user exists and the phone number is not used by anyone else
            if (logUser != null && existingUser == null)
            {
                return Json(new { success = true, message = "Phone number is valid." });
            }

            // If the phone number is already used by another user
            if (existingUser != null)
            {
                return Json(new { success = false, message = "This number is linked to another account. Please try a different one." });
            }

            return Json(new { success = true, message = "Phone number is valid." });
        }
        [Authorize]
        public async Task<ActionResult> EventDetails(int? eventId)
        {
            try
            {
                var checkEventStatus = db.OrgEvents.Where(m => m.eventId == eventId).FirstOrDefault();
                if (checkEventStatus == null)
                {
                    return RedirectToAction("Index", "Page");
                }
                if (checkEventStatus.status.Equals(2) || checkEventStatus.dateEnd <= DateTime.Now)
                {
                    return RedirectToAction("Index", "Page");
                }
                var recommendedEvents = await _volunteerManager.RunRecommendation(UserId);

                var filteredEvent = new List<vw_ListOfEvent>();
                foreach (var recommendedEvent in recommendedEvents)
                {
                    var matchedEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == recommendedEvent.EventID).ToList();
                    filteredEvent.AddRange(matchedEvents);
                }


                var checkEventID = _listsOfEvent.Get(eventId);
                if (checkEventID != null)
                {
                    var getVolInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
                    var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
                    var getOrgInfo = db.OrgEvents.Where(m => m.eventId == eventId).FirstOrDefault();
                    var getInfo = _orgInfo.GetAll().Where(m => m.userId == getOrgInfo.userId).ToList();
                    var getSkillRequirmenet = _skillRequirement.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
                    var getEvent = _orgEvents.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
                    var getOrgOtherEvent = db.sp_OtherEvent(getOrgInfo.userId).ToList();
                    var getEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == getOrgInfo.eventId).ToList();
                    var getVolunteers = _volunteers.GetAll().Where(m => m.eventId == eventId).ToList();
                    var listofUserDonated = db.UserDonated.Where(m => m.eventId == eventId).ToList();
                    var volunteerStatusEvent = _volunteersStatusEvent.GetAll().Where(m => m.userId == UserId && m.eventId == eventId).ToList();

                    var eventStart = getEvent.Where(m => m.eventId == eventId).Select(m => m.dateStart).FirstOrDefault();
                    //if (eventStart < DateTime.Now)
                    //{
                    //    _volunteerManager.RemoveVolunteerFromVolunteerByUserIdAndEventId(UserId, eventId);
                    //}

                    var volunteer = _volunteerManager.GetVolunteerByUserId(UserId, (int)eventId);
                    var c = db.sp_matchSkill(UserId, eventId).ToList().Count();
                    var indexModel = new Lists()
                    {
                        volunteersInfo = getVolInfo,
                        picture = getProfile,
                        orgInfos = getInfo,
                        detailsSkillRequirement = getSkillRequirmenet,
                        skills = _skills.GetAll().ToList(),
                        detailsEventImageOne = _eventImages.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList(),
                        detailsEventImage = _eventImages.GetAll().ToList(),
                        orgEvents = getEvent,
                        orgOtherEvent = getOrgOtherEvent,
                        listOfEventsOne = getEvents,
                        listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
                        volunteers = getVolunteers,
                        listofUserDonated = listofUserDonated,
                        volunteersStatusEvent = _volunteers.GetAll().Where(m => m.eventId == eventId).ToList(),
                        matchSkill = db.sp_matchSkill(UserId, eventId).ToList(),
                        volunteer = volunteer,
                        volunteersSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList(),
                    };
                    return View(indexModel);
                }
                else
                {
                    return RedirectToAction("../Page/Index"); //Error
                }
            }
            catch (Exception)
            {
                return RedirectToAction("../Page/Index"); //Error
            }
        }
        [HttpPost]
        public JsonResult AcceptInvite(int eventId/*, string skill*/)
        {
            try
            {
                var checkVolunteer = _volunteers.GetAll().Where(m => m.userId == UserId && m.eventId == eventId && m.Status == 2).FirstOrDefault();
                var checkDateOrgEvents = _orgEvents.GetAll().Where(m => m.eventId == eventId).FirstOrDefault();

                // Check if the user has already accepted an invitation for this event
                if (!checkVolunteer.Status.Equals(2))
                {
                    return Json(new { success = false, message = "Already accepted invitation" });
                }

                // Get list of events the user has already applied for
                var listUserEvents = db.sp_UserListEvent(UserId).ToList();

                // Convert to DateTime with only the date part
                var checkEventStartDate = checkDateOrgEvents.dateStart?.Date;
                var checkEventEndDate = checkDateOrgEvents.dateEnd?.Date;

                // Check for conflicting event dates
                foreach (var userEvent in listUserEvents)
                {
                    var userEventStartDate = userEvent.dateStart?.Date;
                    var userEventEndDate = userEvent.dateEnd?.Date;

                    if (userEventStartDate == null || userEventEndDate == null)
                    {
                        continue; // Skip if the event dates are null
                    }

                    if (!(checkEventEndDate < userEventStartDate || checkEventStartDate > userEventEndDate))
                    {
                        if (userEvent.Volunteer_Status == 0)
                        {
                            return Json(new { success = false, message = "Conflict with another registered event" });
                        }
                        else if (userEvent.Volunteer_Status == 1)
                        {
                            return Json(new { success = false, message = "Conflict with another applied event" });
                        }
                    }
                }

                var getEventRequiredSkills = _skillRequirement.GetAll().Where(m => m.eventId == eventId).Select(m => m.skillId).ToList();
                var volSkill = _volunteerSkills.GetAll().Where(m => m.userId == UserId).Select(m => m.skillId).ToList();

                bool skillMatch = getEventRequiredSkills.Any(skll => volSkill.Contains(skll));

                if (!skillMatch)
                {
                    return Json(new { success = false, message = "Your skills do not match the requirements" });
                }
                //var selectedSkillID = db.Skills.Where(m => m.skillName == skill).Select(m => m.skillId).FirstOrDefault();
                db.sp_AcceptAndUpdateVolunteerStatus(UserId, eventId);

                var user = _organizationManager.GetVolunteerInfoByUserId(UserId);

                if (_organizationManager.SentNotif((int)checkDateOrgEvents.userId, UserId, checkDateOrgEvents.eventId, "Accept Invitation", $"Volunteer {user.fName} has accepted your invitation.", 0, ref ErrorMessage) != ErrorCode.Success)
                {
                    return Json(new { success = false, message = "Cant send notification" });
                }

                return Json(new { success = true, message = "Invitation accepted" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error accepting invitation!" });
            }
        }
        [HttpPost]
        public JsonResult DeclineInvite(int eventId)
        {
            try
            {
                db.sp_CancelRequest(eventId, UserId);

                var user = _organizationManager.GetVolunteerInfoByUserId(UserId);
                var checkDateOrgEvents = _organizationManager.GetEventByEventId(eventId);

                if (_organizationManager.SentNotif((int)checkDateOrgEvents.userId, UserId, checkDateOrgEvents.eventId, "Decline Invitation", $"Volunteer {user.fName} has declined your invitation.", 0, ref ErrorMessage) != ErrorCode.Success)
                {
                    return Json(new { success = false, message = "Cant send notification" });
                }

                return Json(new { success = true, message = "Invitation declined" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "error message" });
            }
        }
        [HttpPost]
        public JsonResult ApplyVolunteer(int eventId/*, string skill*/)
        {
            try
            {
                var checkVolunteer = _volunteers.GetAll().Where(m => m.userId == UserId && m.eventId == eventId).FirstOrDefault();
                var checkDateOrgEvents = _orgEvents.GetAll().Where(m => m.eventId == eventId).FirstOrDefault();

                // Check if the user has already applied for this event
                if (checkVolunteer != null)
                {
                    return Json(new { success = false, message = "Already applied" });
                }

                // Get list of events the user has already applied for
                var listUserEvents = db.sp_UserListEvent(UserId).Where(m => m.Event_Status != 2).ToList();

                // Convert to DateTime with only the date part
                var checkEventStartDate = checkDateOrgEvents.dateStart?.Date;
                var checkEventEndDate = checkDateOrgEvents.dateEnd?.Date;

                // Check for conflicting event dates
                foreach (var userEvent in listUserEvents)
                {
                    var userEventStartDate = userEvent.dateStart?.Date;
                    var userEventEndDate = userEvent.dateEnd?.Date;

                    if (userEventStartDate == null || userEventEndDate == null)
                    {
                        continue; // Skip if the event dates are null
                    }

                    if (!(checkEventEndDate < userEventStartDate || checkEventStartDate > userEventEndDate))
                    {
                        if (userEvent.Volunteer_Status == 0)
                        {
                            return Json(new { success = false, message = "Conflict with another applied event" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Conflict with another registered event" });
                        }
                    }
                }

                var getEventRequiredSkills = _skillRequirement.GetAll().Where(m => m.eventId == eventId).ToList();
                var volSkill = _volunteerSkills.GetAll().Where(m => m.userId == UserId).Select(m => m.skillId).ToList();

                //===========================================================================
                //Volunteers apply = new Volunteers();
                //foreach (var eventReq in getEventRequiredSkills)
                //{
                //    apply = new Volunteers()
                //    {
                //        userId = UserId,
                //        eventId = eventId,
                //        Status = 0,
                //        skillId = db.Skills.Where(m => m.skillId == eventReq.skillId).Select(m => m.skillId).FirstOrDefault(),
                //        appliedAt = DateTime.Now
                //    };
                //    _volunteers.Create(apply);
                //}
                //===========================================================================


                bool skillMatch = getEventRequiredSkills.All(skll => volSkill.Contains(skll.skillId));

                if (!skillMatch)
                {
                    return Json(new { success = false, message = "Your skills do not match the requirements" });
                }

                var apply = new Volunteers()
                {
                    userId = UserId,
                    eventId = eventId,
                    Status = 0,
                    //skillId = db.Skills.Where(m => m.skillName == skill).Select(m => m.skillId).FirstOrDefault(),
                    appliedAt = DateTime.Now
                };

                //var updateVolunteerNeeded = db.OrgEvents.Where(m => m.eventId == eventId).FirstOrDefault();

                if (checkVolunteer == null)
                {
                    var organizationId = checkDateOrgEvents.userId;
                    _volunteers.Create(apply);

                    _organizationManager.SentNotif((int)organizationId, UserId, eventId, "New Applicant", $"A new volunteer has applied for your event (Event Name: {checkDateOrgEvents.eventTitle}).", 0,  ref ErrorMessage );

                    return Json(new { success = true, message = "Application sent!" });
                }
                else
                {
                    return Json(new { success = false, message = "Already apply" });
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
            var orgInfo = _organizationManager.GetOrgInfoByUserId(donation.userId);
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
        public async Task<JsonResult> DonateNow(int eventId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    return Json(new { success = false, message = "Donation amount must be greater than zero." });
                }

                var referenceNumber = Guid.NewGuid().ToString();
                var checkoutUrl = await CreatePayMongoCheckoutSession(amount, "Donation for event #" + eventId + " - Reference No. " + referenceNumber, eventId, referenceNumber);

                if (checkoutUrl != null)
                {
                    return Json(new { success = true, checkoutUrl = checkoutUrl });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create checkout session. Please try again." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error in DonateNow: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }
        private async Task<string> CreatePayMongoCheckoutSession(decimal amount, string description, int eventId, string referencereferenceNumber)
        {          
            var client = new RestClient("https://api.paymongo.com/v1/checkout_sessions");
            var request = new RestRequest();
            request.Method = Method.Post;

            var secretKey = "sk_test_gvQ3WTM1Acco8AGhp35zT1b1"; // Replace with your actual secret key
            var encodedSecretKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretKey}:"));
            request.AddHeader("Authorization", $"Basic {encodedSecretKey}");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                data = new
                {
                    attributes = new
                    {
                        line_items = new[]
                        {
                    new
                    {                       
                        amount = (int)(amount * 100), // Amount in centavos
                        currency = "PHP",
                        name = "Donation",
                        description = description,
                        quantity = 1
                    }
                },
                        payment_method_types = new[] { "card", "gcash", "grab_pay" },
                        send_email_receipt = false,
                        show_description = true,
                        description = description,
                        cancel_url = Url.Action("PaymentFailed", "Volunteer", new { eventId = eventId, amount = amount }, Request.Url.Scheme),
                        success_url = Url.Action("PaymentSuccess", "Volunteer", new { eventId = eventId, amount = amount, referenceNumber = referencereferenceNumber }, Request.Url.Scheme) // Passing eventId and amount                       
                    }
                }
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var responseData = JsonConvert.DeserializeObject<dynamic>(response.Content);
                string checkoutUrl = responseData.data.attributes.checkout_url;
                return checkoutUrl;
            }
            else
            {
                // Log the error for debugging
                var errorContent = response.Content;
                System.Diagnostics.Debug.WriteLine($"PayMongo Error Response: {errorContent}");
                return null;
            }
        }
        [HttpPost]
        public async Task<ActionResult> PayMongoWebhook()
        {
            var signatureHeader = Request.Headers["Paymongo-Signature"];
            var webhookSecretKey = "whsk_test_your_webhook_secret_key"; // Replace with your actual webhook secret key

            // Read the request body
            string json;
            using (var reader = new StreamReader(Request.InputStream))
            {
                json = await reader.ReadToEndAsync();
            }

            // Verify the webhook signature
            if (!VerifySignature(signatureHeader, json, webhookSecretKey))
            {
                // Invalid signature
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Deserialize the webhook event
            var webhookEvent = JsonConvert.DeserializeObject<dynamic>(json);

            // Extract the event type
            string eventType = webhookEvent.data.attributes.type.ToString();

            if (eventType == "checkout.session_paid")
            {
                // Payment was successful
                string referenceNumber = webhookEvent.data.attributes.data.attributes.reference_number.ToString();
                int donationId = int.Parse(referenceNumber);

                var donation = db.UserDonated.Find(donationId);
                if (donation != null)
                {
                    donation.Status = 1;
                    db.SaveChanges();
                }
            }

            // Return a 200 OK response to acknowledge receipt of the webhook
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        private bool VerifySignature(string signatureHeader, string payload, string secret)
        {
            if (string.IsNullOrEmpty(signatureHeader))
            {
                return false;
            }

            var signatures = signatureHeader.Split(',');

            foreach (var sig in signatures)
            {
                var parts = sig.Split('=');
                if (parts.Length == 2 && parts[0].Trim() == "v1")
                {
                    var expectedSignature = parts[1].Trim();

                    // Compute HMAC SHA256 hash of the payload using the webhook secret key
                    var secretBytes = Encoding.UTF8.GetBytes(secret);
                    var payloadBytes = Encoding.UTF8.GetBytes(payload);
                    using (var hmac = new HMACSHA256(secretBytes))
                    {
                        var hash = hmac.ComputeHash(payloadBytes);
                        var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant().Trim();

                        if (computedSignature == expectedSignature)
                        {
                            return true; // Signature is valid
                        }
                    }
                }
            }
            return false; // Signature is invalid
        }
        public async Task<ActionResult> PaymentSuccess(int eventId, decimal amount, string referenceNumber)
        {

            var recommendedEvents = await _volunteerManager.RunRecommendation(UserId);

            var filteredEvent = new List<vw_ListOfEvent>();
            foreach (var recommendedEvent in recommendedEvents)
            {
                var matchedEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == recommendedEvent.EventID).ToList();
                filteredEvent.AddRange(matchedEvents);
            }

            // Save the donation to the database
            var donation = new UserDonated
            {
                referenceNum = referenceNumber,
                userId = UserId, // User who made the donation
                eventId = eventId,
                amount = amount,
                donatedAt = DateTime.Now,
                Status = 1
            };

            db.UserDonated.Add(donation);
            db.SaveChanges(); // Save the donation to the database

            // Find the organization associated with the event
            var organization = db.OrgEvents
                                 .Where(o => o.eventId == eventId)
                                 .FirstOrDefault();

            if (organization != null)
            {
                // Save the notification for the organization
                var notification = new Notification
                {
                    userId = organization.userId, // Notify the organization
                    senderUserId = UserId, // The user who donated
                    relatedId = eventId,
                    type = "Donation",
                    content = $"You have received a donation of {donation.amount} for event #{eventId}.",
                    broadcast = 0, // Not a broadcast
                    status = 0, // Assuming 1 is the status for a new notification
                    createdAt = DateTime.Now,
                    readAt = null // Initially unread
                };

                db.Notification.Add(notification);
                db.SaveChanges(); // Save the notification
            }

            // Now proceed with loading the user's profile information
            var getUserAccount = db.UserAccount.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var events = _organizationManager.GetEventByEventId(eventId);

            var getUniqueSkill = db.sp_GetSkills(UserId).ToList();
            if (getProfile.Count() <= 0)
            {
                var defaultPicture = new ProfilePicture
                {
                    userId = UserId,
                    profilePath = "default.jpg"
                };
                _profilePic.Create(defaultPicture);

                getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            }
            var listModel = new Lists()
            {
                userAccounts = getUserAccount,
                volunteersInfo = getVolunteerInfo,
                volunteersSkills = getVolunteerSkills,
                uniqueSkill = getUniqueSkill,
                picture = getProfile,
                CreateEvents = events,
                skills = _skills.GetAll().ToList(),
                listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
                detailsEventImage = _eventImages.GetAll().ToList()
            };

            return View(listModel);
        }

        // Payment failed handler
        public async Task<ActionResult> PaymentFailed(int eventId, decimal amount)
        {
            var recommendedEvents = await _volunteerManager.RunRecommendation(UserId);

            var filteredEvent = new List<vw_ListOfEvent>();
            foreach (var recommendedEvent in recommendedEvents)
            {
                var matchedEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == recommendedEvent.EventID).ToList();
                filteredEvent.AddRange(matchedEvents);
            }
            // Save the donation to the database
            var donation = new UserDonated
            {
                userId = UserId, // User who made the donation
                eventId = eventId,
                amount = amount,
                donatedAt = DateTime.Now,
                Status = 0
            };

            db.UserDonated.Add(donation);
            db.SaveChanges(); // Save the donation to the database

            var getUserAccount = db.UserAccount.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var events = _organizationManager.GetEventByEventId(eventId);

            var getUniqueSkill = db.sp_GetSkills(UserId).ToList();
            if (getProfile.Count() <= 0)
            {
                var defaultPicture = new ProfilePicture
                {
                    userId = UserId,
                    profilePath = "default.jpg"
                };
                _profilePic.Create(defaultPicture);

                getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            }
            var listModel = new Lists()
            {
                userAccounts = getUserAccount,
                volunteersInfo = getVolunteerInfo,
                volunteersSkills = getVolunteerSkills,
                uniqueSkill = getUniqueSkill,
                CreateEvents = events,
                picture = getProfile,
                skills = _skills.GetAll().ToList(),
                listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
                detailsEventImage = _eventImages.GetAll().ToList()
            };

            return View(listModel);
        }
        [Authorize]
        public async Task<ActionResult> DonationHistory()
        {

            var orgEvents = new List<OrgEvents>();
            foreach (var ev in _userDonated.GetAll())
            {
                var evnt = db.OrgEvents.Where(m => m.eventId == ev.eventId);

                orgEvents.AddRange(evnt);
            }

            var recommendedEvents = await _volunteerManager.RunRecommendation(UserId);

            var filteredEvent = new List<vw_ListOfEvent>();
            foreach (var recommendedEvent in recommendedEvents)
            {
                var matchedEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == recommendedEvent.EventID).ToList();
                filteredEvent.AddRange(matchedEvents);
            }

            var listModel = new Lists()
            {
                picture = db.ProfilePicture.Where(m => m.userId == UserId).ToList(),
                volunteersInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList(),
                listofUserDonated = db.UserDonated.Where(m => m.userId == UserId).ToList(),
                orgEvents = orgEvents,
                userDonatedInformations = db.sp_GetUserDonatedInformations(UserId).ToList(),
                listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
                detailsEventImage = _eventImages.GetAll().ToList()
            };
            return View(listModel);
        }
        [Authorize]
        public async Task<ActionResult> Participate(string section = null)
        {
            try
            {
                // Fetching all accepted and pending events for the volunteer
                var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
                var acceptedEvents = _volunteers.GetAll().Where(m => m.userId == UserId && m.Status == 1).ToList();
                //var pendingEvents = _volunteers.GetAll().Where(m => m.userId == UserId && m.Status == 0).ToList();
                var getOrgImages = _eventImages.GetAll().ToList();
                var userProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

                List<Volunteers> pendings = _volunteerManager.GetVolunteersEventPendingByUserId(UserId);
                List<Volunteers> accepted = _volunteerManager.GetVolunteersEventParticipateByUserId(UserId);

                var recommendedEvents = await _volunteerManager.RunRecommendation(UserId);

                var filteredEvent = new List<vw_ListOfEvent>();
                foreach (var recommendedEvent in recommendedEvents)
                {
                    var matchedEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == recommendedEvent.EventID).ToList();
                    filteredEvent.AddRange(matchedEvents);
                }

                var indexModel = new Lists()
                {
                    picture = userProfile,
                    volunteers = acceptedEvents,
                    orgEvents = accepted.OrderByDescending(m => m.applyVolunteerId).Select(e => _orgEvents.GetAll().FirstOrDefault(o => o.eventId == e.eventId)).ToList(),
                    orgEventHistory = db.OrgEvents.Where(m => m.userId == UserId && m.status == 2).ToList(),
                    pendingOrgDetails = pendings.OrderByDescending(m => m.applyVolunteerId).Select(e => _pendingOrgDetails.GetAll().FirstOrDefault(p => p.eventId == e.eventId)).ToList(),
                    volunteersInfo = getVolunteerInfo,
                    volunteersHistories = db.sp_VolunteerHistory(UserId).ToList(),
                    rating = db.Rating.Where(m => m.userId == UserId).ToList(),
                    detailsEventImage = getOrgImages,
                    listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
                    detailsSkillRequirement = _skillRequirement.GetAll(),
                    allSkill = db.Skills.ToList()
                };

                ViewBag.SectionToShow = section;

                return View(indexModel);
            }
            catch (Exception)
            {
                return RedirectToAction("GeneralSkill");
            }
        }

        [HttpPost]
        public ActionResult LeaveEvent(int eventId)
        {
            try
            {
                var updateVol = _volunteers.GetAll().Where(m => m.userId == UserId && m.eventId == eventId).FirstOrDefault();
                var events = _organizationManager.GetEventByEventId(eventId);

                if (updateVol != null)
                {
                    db.sp_LeaveEvent(updateVol.eventId, updateVol.userId);

                    // Save the notification for the organization
                    var notification = new Notification
                    {
                        userId = events.userId, // Notify the organization
                        senderUserId = UserId, // The user who donated
                        relatedId = eventId,
                        type = "Leave Event",
                        content = $"{updateVol.UserAccount.email} Left {events.eventTitle} Event.",
                        broadcast = 0, // Not a broadcast
                        status = 0, // Assuming 1 is the status for a new notification
                        createdAt = DateTime.Now,
                        readAt = null // Initially unread
                    };

                    db.Notification.Add(notification);
                    db.SaveChanges(); // Save the notification

                    return Json(new { success = true, message = "Left Successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Error" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "error message" });
            }
        }
        [HttpPost]
        public ActionResult CancelRequest(int eventId)
        {
            try
            {
                var evnt = _organizationManager.GetEventByEventId(eventId);
                var vol = _organizationManager.GetUserByUserId(UserId);

                if (evnt != null)
                {
                    var orga = _organizationManager.GetUserByUserId((int)evnt.userId);

                    db.sp_CancelRequest(eventId, UserId);

                    if (_organizationManager.SentNotif(orga.userId, UserId, eventId, "Cancel Application", $"Volunteer {vol.email} canceled his application og event {evnt.eventTitle}!", 0, ref ErrorMessage) != ErrorCode.Success)
                    {
                        return Json(new { success = false, message = "Raquest cancel failed" });
                    }

                    return Json(new { success = true, message = "Request Cancelled" });
                }

                return Json(new { success = true, message = "Request Cancelled" });

            }
            catch (Exception)
            {
                return Json(new { success = false, message = "error message" });
            }
        }
        [Authorize]
        public async Task<ActionResult> OrganizationProfile(int userId)
        {
            var userProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();

            var getOrgUserId = db.OrgEvents.Where(m => m.eventId == userId).Select(m => m.userId).FirstOrDefault();
            var getOrgInfo = _organizationManager.GetOrgInfoByUserId(userId);

            var orgEvents = _organizationManager.GetOrgEventsByUserId(userId);
            var orgImage = new List<OrgEventImage>();
            foreach (var image in orgEvents)
            {
                var orgEvenImage = _organizationManager.GetEventImageByEventId(image.eventId);

                orgImage.Add(orgEvenImage);
            }

            var recommendedEvents = await _volunteerManager.RunRecommendation(UserId);

            var filteredEvent = new List<vw_ListOfEvent>();
            foreach (var recommendedEvent in recommendedEvents)
            {
                var matchedEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == recommendedEvent.EventID).ToList();
                filteredEvent.AddRange(matchedEvents);
            }

            var indexModel = new Lists()
            {
                picture = userProfile,
                volunteersInfo = getVolunteerInfo,
                OrgInfo = getOrgInfo,
                detailsEventImage = orgImage,
                getAllOrgEvent = orgEvents,
                listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
            };

            return View(indexModel);
        }
        [HttpPost]
        public JsonResult OpenNotification(int notificationId)
        {
            try
            {
                // Fetch the notification from the database
                var notification = db.Notification.FirstOrDefault(n => n.notificationId == notificationId);

                if (notification != null)
                {
                    // Mark the notification as read
                    notification.status = 1;
                    db.SaveChanges();

                    // Optionally, get the URL to redirect the user
                    string redirectUrl = GetRedirectUrlForNotification(notification);

                    return Json(new { success = true, redirectUrl = redirectUrl });
                }
                else
                {
                    return Json(new { success = false, message = "Notification not found." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                // Return an error response
                return Json(new { success = false, message = ex.Message });
            }
        }
        private string GetRedirectUrlForNotification(Notification notification)
        {
            // Logic to determine redirect URL based on notification type or content
            // For example:
            if (notification.type == "Invitation")
            {
                return Url.Action("EventDetails", "Volunteer", new { eventId = notification.relatedId });
            }
            else if (notification.type == "Donation")
            {
                return Url.Action("EventDetails", "Volunteer", new { eventId = notification.relatedId });
            }
            else if (notification.type == "Event Update")
            {
                return Url.Action("EventDetails", "Volunteer", new { eventId = notification.relatedId });
            }
            else if (notification.type == "Create Event")
            {
                return Url.Action("EventDetails", "Volunteer", new { eventId = notification.relatedId });
            }
            else if (notification.type == "Acceptance")
            {
                return Url.Action("EventDetails", "Volunteer", new { eventId = notification.relatedId });
            }
            // Default to null if no redirection is needed
            return null;
        }
    }
}