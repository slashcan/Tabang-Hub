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
        private BaseRepository<OrgAccount> _orgAcc;
        private BaseRepository<OrgId> _orgId;
        private BaseRepository<UserRoles> _userRoles;
        public UserManager() 
        {
            _userAcc = new BaseRepository<UserAccount>();
            _orgAcc = new BaseRepository<OrgAccount>();
            _orgId = new BaseRepository<OrgId>();
            _userRoles = new BaseRepository<UserRoles>();
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

        public ErrorCode Register(UserAccount u, UserRoles r, ref String errMsg)
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
            r.userId = u.userId;
            r.userRole = u.roleId;
            if (_userRoles.Create(r, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }

        public ErrorCode OrgRegister(OrgAccount u, OrgId o, UserRoles r, ref String errMsg)
        {
            u.roleId = 2;
            u.status = 0;

            if (GetUserByEmail(u.orgEmail) != null)
            {
                errMsg = "Email Already Exist";
                return ErrorCode.Error;
            }

            if (_orgId.Create(o, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            u.orgImageId = o.imageId;

            if (_orgAcc.Create(u, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            r.userId = u.orgId;
            r.userRole = u.roleId;
            if (_userRoles.Create(r, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
    }
}