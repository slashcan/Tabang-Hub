using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class UserManager
    {
        private BaseRepository<UserAccount> _userAcc;
        private BaseRepository<VolunteerInfo> _volunteerInfo;
        private BaseRepository<OrgInfo> _orgInfo;
        private BaseRepository<OrgValidation> _orgValid;
        private BaseRepository<VolunteerSkill> _volunteerSkills;
        private BaseRepository<ProfilePicture> _profilePic;
        private BaseRepository<UserRole> _userRoles;
        public UserManager() 
        {
            _userAcc = new BaseRepository<UserAccount>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _orgValid = new BaseRepository<OrgValidation>();
            _volunteerSkills = new BaseRepository<VolunteerSkill>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _userRoles = new BaseRepository<UserRole>();

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

        public ErrorCode Register(UserAccount u, VolunteerInfo v, UserRole r, ref String errMsg)
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
            v.userId = u.userId;          
            if (_volunteerInfo.Create(v, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            r.userId = u.userId;
            r.userRole1 = u.roleId;
            if (_userRoles.Create(r, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }

        public ErrorCode OrgRegister(UserAccount u, OrgInfo o, OrgValidation ov, UserRole r, ref String errMsg)
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
            r.userRole1 = u.roleId;
            if (_userRoles.Create(r, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            o.userId = u.userId;
            if (_orgInfo.Create(o, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }          
            return ErrorCode.Success;
        }
        public ErrorCode UpdateUserStatus(int userId, short newStatus, ref string errMsg)
        {
            // First, retrieve the user account by its ID
            var user = _userAcc.Get(userId);

            if (user != null)
            {
                // Update the status field
                user.status = newStatus;

                // Now, call the Update method to save the changes
                return (ErrorCode)_userAcc.Update(userId, user, out errMsg);
            }
            else
            {
                errMsg = "User not found";
                return ErrorCode.Error;
            }
        }
    }
}