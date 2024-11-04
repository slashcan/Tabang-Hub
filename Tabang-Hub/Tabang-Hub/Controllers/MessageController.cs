using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Controllers
{
    public class MessageController : BaseController
    {
        // GET: Message
        public ActionResult Message()
        {
            try
            {

                var user = _organizationManager.GetUserByUserId(UserId);
                ViewBag.UserId = UserId;
                if (user != null && user.roleId == 2)
                {
                    var orgInfo = _organizationManager.GetOrgInfoByUserId(user.userId);
                    var listOfGc = new List<sp_ListOfGc_Result>();
                    var eventImage = new List<OrgEventImage>();

                    var gc = _messageManager.GetGroupChatByUserId(user.userId);

                    foreach (var g in gc)
                    {
                        var gcItem = new sp_ListOfGc_Result
                        {
                            groupChatId = g.groupChatId,
                            userId = user.userId,
                            eventId = g.eventId,
                            eventTitle = g.OrgEvents.eventTitle,
                        };

                        var image = _messageManager.GetEventImageByEventId((int)g.eventId);

                        listOfGc.Add(gcItem);
                        eventImage.Add(image);
                    }
                    var indexModel = new Lists()
                    {
                        userAccount = user,
                        OrgInfo = orgInfo,   
                        detailsEventImage = eventImage,
                        listOfGc = listOfGc,
                    };

                    return View(indexModel);
                }
                else
                {
                    var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

                    var checkVolunteers = db.Volunteers.Where(m => m.userId == UserId).FirstOrDefault();

                    var getOrgEvent = _orgEvents.GetAll().Where(m => m.eventId == checkVolunteers.eventId).ToList();

                    var eventImage = new List<OrgEventImage>();

                    var listOfGC = db.sp_ListOfGc(UserId).ToList();

                    foreach (var gc in listOfGC)
                    {
                        var image = _messageManager.GetEventImageByEventId((int)gc.eventId);

                        eventImage.Add(image);
                    }


                    var indexModel = new Lists()
                    {
                        picture = getProfile,
                        volunteersInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList(),
                        detailsEventImage = eventImage,
                        orgEvents = getOrgEvent,
                        listOfGc = listOfGC
                    };

                    return View(indexModel);
                }         
            }
            catch (Exception)
            {
                var indexModel = new Lists()
                {
                    picture = db.ProfilePicture.Where(m => m.userId == UserId).ToList(),
                    volunteersInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList(),
                    detailsEventImage = null,
                    orgEvents = null,
                    listOfGc = null
                };

                return View(indexModel);
            }
        }
    }
}