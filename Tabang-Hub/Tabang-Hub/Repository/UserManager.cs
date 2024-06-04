using System;
using System.Collections.Generic;
<<<<<<< Updated upstream
using System.Linq;
using System.Web;
=======
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
>>>>>>> Stashed changes
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class UserManager
    {
        private BaseRepository<UserAccount> _userAcc;
<<<<<<< Updated upstream
        public UserManager() 
        {
            _userAcc = new BaseRepository<UserAccount>();
=======
        private BaseRepository<VolunteerInfo> _volunteerInfo;
        private BaseRepository<OrgInfo> _orgInfo;
        private BaseRepository<OrgValidation> _orgValid;
        private BaseRepository<Skills> _volunteerSkills;
        private BaseRepository<ProfilePicture> _profilePic;
        private BaseRepository<UserRoles> _userRoles;
        public UserManager() 
        {
            _userAcc = new BaseRepository<UserAccount>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _orgValid = new BaseRepository<OrgValidation>();
            _volunteerSkills = new BaseRepository<Skills>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _userRoles = new BaseRepository<UserRoles>();

>>>>>>> Stashed changes
        }

        public UserAccount GetUserByEmail(String email)
        {
            return _userAcc._table.Where(m => m.email == email).FirstOrDefault();
        }

        public ErrorCode Login(String email, String password, ref String errMsg)
        {
            var userLogin = GetUserByEmail(email);
            if (userLogin == null)
            {
                errMsg = "User not exist!";
                return ErrorCode.Error;
            }

            if (!userLogin.password.Equals(password))
            {
                errMsg = "Password is Incorrect";
                return ErrorCode.Error;
            }

            // user exist
            errMsg = "Login Successful";
            return ErrorCode.Success;
        }

<<<<<<< Updated upstream
        public ErrorCode Register(UserAccount u, ref String errMsg)
=======
        public ErrorCode Register(UserAccount u, VolunteerInfo v, UserRoles r, ref String errMsg)
>>>>>>> Stashed changes
        {
            u.roleId = 1;
            u.status = 0;

            if (GetUserByEmail(u.email) != null)
            {
                errMsg = "Email Already Exist";
                return ErrorCode.Error;
            }

            if (_userAcc.Create(u, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
<<<<<<< Updated upstream
=======
            v.userId = u.userId;          
            if (_volunteerInfo.Create(v, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            r.userId = u.userId;
            r.userRole = u.roleId;
            if (_userRoles.Create(r, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }

        public ErrorCode OrgRegister(UserAccount u, OrgInfo o, OrgValidation ov, UserRoles r, ref String errMsg)
        {
            u.roleId = 2;
            u.status = 0;

            if (GetUserByEmail(u.email) != null)
            {
                errMsg = "Email Already Exist";
                return ErrorCode.Error;
            }

            if (_userAcc.Create(u, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            ov.userId = u.userId;
            if (_orgValid.Create(ov, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            r.userId = u.userId;
            r.userRole = u.roleId;
            if (_userRoles.Create(r, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            o.userId = u.userId;
            if (_orgInfo.Create(o, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }          
>>>>>>> Stashed changes
            return ErrorCode.Success;
        }
    }
}