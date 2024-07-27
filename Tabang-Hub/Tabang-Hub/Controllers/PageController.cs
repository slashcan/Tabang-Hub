using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tabang_Hub.Repository;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Controllers
{
    [Authorize(Roles = "Volunteer,Organization,Admin")]
    public class PageController : BaseController
    {
        // GET: Page
        public ActionResult Index()
        {
            var user = _userManager.GetUserByEmail(User.Identity.Name);
            if (user != null)
            {
                switch (user.roleId)
                {
                    case 1:

                        var getInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
                        var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
                        var getSkills = _skills.GetAll().ToList();
                        var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

                        var orgEventsSelectId = _orgEvents.GetAll().Where(m => m.targetAmount != null).Select(m => m.eventId).ToList();
                        var orgEvents = _orgEvents.GetAll().Where(m => m.targetAmount != null).ToList();

                        var getUserDonated = new List<UserDonated>();
                        foreach (var eventId in orgEventsSelectId)
                        {
                            getUserDonated = _userDonated.GetAll().Where(m => m.eventId == eventId).ToList();
                        }

                        var indexModel = new Lists()
                        {
                            volunteersInfo = getInfo,
                            volunteersSkills = getVolunteerSkills,
                            skills = getSkills,
                            picture = getProfile,
                            listOfEvents = getEvents,
                            volunteers = getVolunteers,
                            orgInfos = getOrgInfo
                        };
                        return View(indexModel);
                    case 2:
                        return RedirectToAction("Index", "Organization");
                    case 3:
                        return RedirectToAction("Index", "Admin");
                }
            }

            return View();
        }
        [AllowAnonymous]
        public ActionResult ChooseRegister()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult RegisterOrg()
        {
            return View();
        }
        [AllowAnonymous]
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

            TempData["email"] = u.email;
            Session["email"] = u.email;
            Session["NewAccountId"] = u.userId;

            Random random = new Random();
            int randomOTP = random.Next(1000, 10000);
            Session["randomOTP"] = randomOTP;

            MailManager sendOTP = new MailManager();
            string subject = "Welcome to our website!";
            string userEmail = u.email;
            string body = $"Welcome to Mobile Legend {randomOTP}";

            string errorResponse = "";

            bool isOTPSent = sendOTP.SendEmail(userEmail, subject, body, ref errorResponse);
            if (isOTPSent)
            {
                return RedirectToAction("Verify");
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
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
                    return View(u);
                }

                if (_userManager.Register(u, v, r, ref ErrorMessage) != ErrorCode.Success)
                {
                    ModelState.AddModelError(String.Empty, ErrorMessage);

                    return View(u);
                }
                TempData["email"] = u.email;
                Session["email"] = u.email;
                Session["NewAccountId"] = u.userId;

                Random random = new Random();
                int randomOTP = random.Next(1000, 10000);
                Session["randomOTP"] = randomOTP;

                MailManager sendOTP = new MailManager();
                string subject = "Welcome to our website!";
                string userEmail = u.email;
                string body = $"Welcome to Mobile Legend {randomOTP}";

                string errorResponse = "";

                bool isOTPSent = sendOTP.SendEmail(userEmail, subject, body, ref errorResponse);
                if (isOTPSent)
                {
                    return RedirectToAction("Verify");
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = $"Error! " + ex.Message;
                return RedirectToAction("Register");
            }

            return RedirectToAction("Login");
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(String email, String password)
        {

            if (_userManager.Login(email, password, ref ErrorMessage) == ErrorCode.Success)
            {
                var user = _userManager.GetUserByEmail(email);

                if (user.status != (Int32)Status.Active)
                {
                    TempData["email"] = email;
                    return RedirectToAction("Index");
                }

                FormsAuthentication.SetAuthCookie(email, false);

                if (user.roleId == 1)
                {
                    return RedirectToAction("Index");
                }
                else if (user.roleId == 2)
                {
                    return RedirectToAction("Index", "Organization");
                }
                else if (user.roleId == 3)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            ViewBag.Error = ErrorMessage;
            return View();
        }
        [AllowAnonymous]
        public ActionResult Verify()
        {
            if (String.IsNullOrEmpty(TempData["email"] as String))
                return RedirectToAction("Login");

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Verify(int otp1, int otp2, int otp3, int otp4)
        {
            var user = _userManager.GetUserByEmail((String)Session["email"]);

            string enteredOTP = $"{otp1}{otp2}{otp3}{otp4}";

            string expectedOTP = Session["randomOTP"].ToString();

            if (enteredOTP == expectedOTP)
            {
                var newAccId = Session["NewAccountID"];
                TempData["SuccessMessage"] = "Email has been verified!";
                _userManager.UpdateUserStatus(user.userId, (Int32)Status.Active, ref ErrorMessage);
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Incorrect OTP. Please try again!";
                return RedirectToAction("Verify");
            }

        }
        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}