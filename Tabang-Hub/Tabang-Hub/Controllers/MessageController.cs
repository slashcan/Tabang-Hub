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
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var getEventProfile = db.OrgEventImage.Select(m => m.eventId).FirstOrDefault();

            var checkVolunteers = db.Volunteers.Where(m => m.userId == UserId).FirstOrDefault();

            var groupChatProfile = db.OrgEventImage.Where(m => m.eventId == checkVolunteers.eventId).ToList();
            var getOrgEvent = _orgEvents.GetAll().Where(m => m.eventId == checkVolunteers.eventId).ToList();

            var listOfGC = db.sp_ListOfGc(UserId).ToList();

            var indexModel = new Lists()
            {
                picture = getProfile,
                volunteersInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList(),
                detailsEventImage = groupChatProfile,
                orgEvents = getOrgEvent,
                listOfGc = listOfGC
            };

            return View(indexModel);
        }
    }
}