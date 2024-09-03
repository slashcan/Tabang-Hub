using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using Tabang_Hub.Contracts;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class VolunteerManager
    {
        public BaseRepository<Skills> _skills;
        public BaseRepository<UserDonated> _userDonated;
        public VolunteerManager() 
        {
            _skills = new BaseRepository<Skills>();
            _userDonated = new BaseRepository<UserDonated>();
        }

        public ErrorCode CreateDonation(UserDonated userDonated, ref String errMsg)
        {
            if (_userDonated.Create(userDonated, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
    }
}