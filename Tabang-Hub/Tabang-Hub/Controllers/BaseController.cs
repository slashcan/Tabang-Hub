using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Repository;

namespace Tabang_Hub.Controllers
{
    public class BaseController : Controller
    {
        public UserManager _userManager;
        public String ErrorMessage;

        public String Email { get { return User.Identity.Name;  } }
        public int UserId { get { return _userManager.GetUserByEmail(Email).userId; } }
        public String UserEmail { get { return _userManager.GetUserByEmail(Email).email; } }
        public BaseController()
        {
           _userManager = new UserManager();
            ErrorMessage = String.Empty;
        }
    }
}