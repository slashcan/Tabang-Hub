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

            var indexModel = new Lists()
            {
                picture = getProfile
            };

            return View(indexModel);
        }
    }
}