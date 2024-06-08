using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Repository;
using Tabang_Hub.ListModel;

namespace Tabang_Hub.Controllers
{
    public class BaseController : Controller
    {
        public TabangHubEntities db;
        public UserManager _userManager;
        public String ErrorMessage;

        public BaseRepository<Skills> _skills;
        public BaseRepository<VolunteerSkill> _volunteerSkills;
        public BaseRepository<VolunteerInfo> _volunteerInfo;
        public BaseRepository<UserAccount> _userAcc;
        public BaseRepository<OrgInfo> _orgInfo;
        public BaseRepository<OrgValidation> _orgValid;
        public BaseRepository<ProfilePicture> _profilePic;
        public BaseRepository<UserRole> _userRoles;

        public String Email { get { return User.Identity.Name;  } }
        public int UserId { get { return _userManager.GetUserByEmail(Email).userId; } }
        public String UserEmail { get { return _userManager.GetUserByEmail(Email).email; } }
        public BaseController()
        {
            db = new TabangHubEntities();
           _userManager = new UserManager();
            ErrorMessage = String.Empty;

            _skills = new BaseRepository<Skills>();
            _volunteerSkills = new BaseRepository<VolunteerSkill>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _userAcc = new BaseRepository<UserAccount>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _orgValid = new BaseRepository<OrgValidation>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _userRoles = new BaseRepository<UserRole>();
        }
    }
}