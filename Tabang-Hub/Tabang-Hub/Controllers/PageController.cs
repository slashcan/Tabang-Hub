using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Controllers
{
    [Authorize(Roles = "Volunteer,Organization,Admin")]
    public class PageController : BaseController
    {
        // GET: Page
        public ActionResult Index()
        {
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
        public ActionResult RegisterOrg(OrgAccount u, OrgId o, UserRoles r, HttpPostedFileBase picture1, HttpPostedFileBase picture2)
        {
            if (picture1 != null && picture1.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(picture1.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/UploadedFiles/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                picture1.SaveAs(serverSavePath);

                o.idPicture1 = inputFileName;
            }
            if (picture2 != null && picture2.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(picture2.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/UploadedFiles/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                picture2.SaveAs(serverSavePath);

                o.idPicture2 = inputFileName;
            }           
            if (_userManager.OrgRegister(u, o, r, ref ErrorMessage) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);                   
            }

            return RedirectToAction("Login");
        }      
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Register(UserAccount u, UserRoles r, String ConfirmPass)
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

                if (_userManager.Register(u, r, ref ErrorMessage) != ErrorCode.Success)
                {
                    ModelState.AddModelError(String.Empty, ErrorMessage);

                    return View(u);
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

                FormsAuthentication.SetAuthCookie(email, false);

                //if (user.roleId == 1)
                //{
                //    return RedirectToAction("Index");
                //}
                //else if (user.roleId == 2)
                //{
                //    //Redirect to Organization
                //}
                //else if (user.roleId == 3)
                //{
                //    //Redirect to Admin
                //}
                //return View();
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}