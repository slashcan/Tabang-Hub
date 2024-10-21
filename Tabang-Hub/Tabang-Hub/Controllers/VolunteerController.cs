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
                volunteersHistories = db.sp_VolunteerHistory(UserId).ToList(),
                rating = db.Rating.Where(m => m.userId == UserId).ToList()
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
                        //Ang user ganahan tang tangon ang skill sa database
                        var skillToRemove = db.VolunteerSkill.Where(m => !skills.Contains(m.skillId) && m.userId == UserId).ToList();

                        foreach (var removeSkill in skillToRemove)
                        {
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
        //public ActionResult GeneralSkill()
        //{
        //    var getEventImages = _eventImages.GetAll().ToList();
        //    var getSkills = _skills.GetAll().ToList();
        //    var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
        //    var getEvents = _listsOfEvent.GetAll().ToList();

        //    var getOrgEvents = _orgEvents.GetAll().FirstOrDefault();
        //    var getOrgInfo = db.OrgInfo.Where(m => m.userId == getOrgEvents.userId).ToList();

        //    var getSkillRequirements = _skillRequirement.GetAll().ToList();

        //    var indexModel = new Lists()
        //    {
        //        skills = getSkills,
        //        picture = getProfile,
        //        listOfEvents = getEvents,
        //        orgInfos = getOrgInfo,
        //        detailsSkillRequirement = getSkillRequirements,
        //        detailsEventImage = getEventImages
        //    };
        //    return View(indexModel);
        //}
        public ActionResult EventDetails(int? eventId)
        {
            try
            {
                var checkEventID = _listsOfEvent.Get(eventId);
                if (checkEventID != null)
                {
                    var getVolInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
                    var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
                    var getOrgInfo = db.OrgEvents.Where(m => m.eventId == eventId).FirstOrDefault();
                    var getInfo = _orgInfo.GetAll().Where(m => m.userId == getOrgInfo.userId).ToList();
                    var getSkillRequirmenet = _skillRequirement.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
                    var getOrgImages = _eventImages.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
                    var getEvent = _orgEvents.GetAll().Where(m => m.eventId == getOrgInfo.eventId).ToList();
                    var getOrgOtherEvent = db.sp_OtherEvent(getOrgInfo.userId).ToList();
                    var getEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == getOrgInfo.eventId).ToList();
                    var getVolunteers = _volunteers.GetAll().Where(m => m.eventId == eventId).ToList();
                    var listofUserDonated = db.UserDonated.Where(m => m.eventId == eventId).ToList();
                    var volunteerStatusEvent = _volunteersStatusEvent.GetAll().Where(m => m.userId == UserId && m.eventId == eventId).ToList();

                    var indexModel = new Lists()
                    {
                        volunteersInfo = getVolInfo,
                        picture = getProfile,
                        orgInfos = getInfo,
                        detailsSkillRequirement = getSkillRequirmenet,
                        skills = _skills.GetAll().ToList(),
                        detailsEventImage = getOrgImages,
                        orgEvents = getEvent,
                        orgOtherEvent = getOrgOtherEvent,
                        listOfEvents = getEvents,
                        volunteers = getVolunteers,
                        listofUserDonated = listofUserDonated,
                        volunteersStatusEvent = volunteerStatusEvent,
                        matchSkill = db.sp_matchSkill(UserId, eventId).ToList(),
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
        public JsonResult ApplyVolunteer(int eventId, string skill)
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
                        if (userEvent.Status == 0)
                        {
                            return Json(new { success = false, message = "Conflict with another applied event" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Conflict with another registered event" });
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

                var apply = new Volunteers()
                {
                    userId = UserId,
                    eventId = eventId,
                    Status = 0,
                    skillId = db.Skills.Where(m => m.skillName == skill).Select(m => m.skillId).FirstOrDefault(),
                    appliedAt = DateTime.Now
                }; 

                //var updateVolunteerNeeded = db.OrgEvents.Where(m => m.eventId == eventId).FirstOrDefault();

                if (checkVolunteer == null)
                {
                    //updateVolunteerNeeded.maxVolunteer = updateVolunteerNeeded.maxVolunteer - 1;
                    //db.SaveChanges();

                    var organizationId = checkDateOrgEvents.userId;

                    // Save the notification to the database
                    var notificationMessage = $"A new volunteer has applied for your event (Event ID: {eventId}).";
                    var notificationType = "New Application";

                    // Instantiate NotificationHub and save the notification
                    var notificationHub = new NotificationHub();
                    notificationHub.SendNotification((int)organizationId, UserId, notificationType, notificationMessage);

                    // Send real-time notification if the organization is online
                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    context.Clients.User(organizationId.ToString()).receiveNotification(notificationType, notificationMessage);
                    _volunteers.Create(apply);

                    return Json(new { success = true, message = "Application sent" });
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
        public async Task<JsonResult> DonateNow(int eventId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    return Json(new { success = false, message = "Donation amount must be greater than zero." });
                }

                var checkoutUrl = await CreatePayMongoCheckoutSession(amount, "Donation for event #" + eventId, eventId);

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
        private async Task<string> CreatePayMongoCheckoutSession(decimal amount, string description, int eventId)
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
                        cancel_url = Url.Action("PaymentFailed", "Volunteer", null, Request.Url.Scheme),
                        success_url = Url.Action("PaymentSuccess", "Volunteer", new { eventId = eventId, amount = amount }, Request.Url.Scheme) // Passing eventId and amount
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
                    donation.Status = "Paid";
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
        public ActionResult PaymentSuccess(int eventId, decimal amount)
        {
            // Save the donation to the database
            var donation = new UserDonated
            {
                userId = UserId, // User who made the donation
                eventId = eventId,
                amount = amount,
                donatedAt = DateTime.Now,
                Status = "Paid"
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
            };

            return View(listModel);
        }

        // Payment failed handler
        public ActionResult PaymentFailed()
        {
            var getUserAccount = db.UserAccount.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

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
            };

            return View(listModel);
        }
        public ActionResult Participate()
        {
            try
            {
                // Fetching all accepted and pending events for the volunteer
                var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
                var acceptedEvents = _volunteers.GetAll().Where(m => m.userId == UserId && m.Status == 1).ToList();
                var pendingEvents = _volunteers.GetAll().Where(m => m.userId == UserId && m.Status == 0).ToList();
                var getOrgImages = _eventImages.GetAll().ToList();
                var userProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

                var indexModel = new Lists()
                {
                    picture = userProfile,
                    volunteers = acceptedEvents,
                    orgEvents = acceptedEvents.Select(e => _orgEvents.GetAll().FirstOrDefault(o => o.eventId == e.eventId)).ToList(),
                    orgEventHistory = db.OrgEventHistory.Where(m => m.userId == UserId).ToList(),
                    pendingOrgDetails = pendingEvents.Select(e => _pendingOrgDetails.GetAll().FirstOrDefault(p => p.eventId == e.eventId)).ToList(),
                    volunteersInfo = getVolunteerInfo,
                    volunteersHistories = db.sp_VolunteerHistory(UserId).ToList(),
                    detailsEventImage = getOrgImages,
                    orgEventImageHistories = db.OrgEventImageHistory.ToList()
                };

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
                        type = "Leave",
                        content = $"{updateVol.UserAccount.email} Left {events.eventTitle} Event.",
                        broadcast = 0, // Not a broadcast
                        status = 0, // Assuming 1 is the status for a new notification
                        createdAt = DateTime.Now,
                        readAt = null // Initially unread
                    };

                    db.Notification.Add(notification);
                    db.SaveChanges(); // Save the notification

                    return Json(new { success = true, message = "Leave successfully" });
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
                db.sp_CancelRequest(eventId, UserId);
                return Json(new { success = true, message = "Cancel Request" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "error message" });
            }
        }
        public ActionResult OrganizationProfile(int userId)
        {
            var userProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();

            var getOrgUserId = db.OrgEvents.Where(m => m.eventId == userId).Select(m => m.userId).FirstOrDefault();
            var getOrgInfo = db.OrgInfo.Where(m => m.userId == userId).ToList();

            var indexModel = new Lists()
            {
                picture = userProfile,
                volunteersInfo = getVolunteerInfo,
                orgInfos = getOrgInfo
            };

            return View(indexModel);
        }      
    }
}