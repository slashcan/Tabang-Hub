using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class AdminManager
    {
        private BaseRepository<vw_VolunteerAccounts> _volunteerAccounts;
        private BaseRepository<vw_OrganizationAccounts> _organizationAccounts;
        private BaseRepository<UserAccount> _userAccount;
        private BaseRepository<VolunteerInfo> _volunteerInfo;
        private BaseRepository<ProfilePicture> _profilePic;
        private BaseRepository<Volunteers> _joinedEvent;
        private BaseRepository<VolunteerSkill> _volunteerSkill;
        private BaseRepository<UserRoles> _userRoles;
        public AdminManager()
        {
            _volunteerAccounts = new BaseRepository<vw_VolunteerAccounts>();
            _organizationAccounts = new BaseRepository<vw_OrganizationAccounts>();
            _userAccount = new BaseRepository<UserAccount>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _joinedEvent = new BaseRepository<Volunteers>();
            _volunteerSkill = new BaseRepository<VolunteerSkill>();
            _userRoles = new BaseRepository<UserRoles>();           
        }

        public List<vw_VolunteerAccounts> GetVolunteerAccounts()
        {
            return _volunteerAccounts.GetAll();
        }
        public List<vw_OrganizationAccounts> GetOrganizationAccounts()
        {
            return _organizationAccounts.GetAll();
        }
        public UserAccount GetUserById(int userId)
        {
            return _userAccount._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public VolunteerInfo GetVolunteerInfoByUserId(int userId)
        {
            return _volunteerInfo._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public List<ProfilePicture> GetProfileByUserId(int userId)
        {
            return _profilePic.GetAll().Where(m => m.userId == userId).ToList();
        }
        public List<Volunteers> GetJoinedEventByUserId(int userId)
        {
            return _joinedEvent.GetAll().Where(m => m.userId == userId).ToList();
        }
        public List<VolunteerSkill> GetVolunteerSkillsByUserId(int userId)
        { 
            return _volunteerSkill.GetAll().Where(m => m.userId == userId).ToList();
        }
        public List<UserRoles> GetRolesByUserId(int userId)
        {
            return _userRoles.GetAll().Where(m => m.userId == userId).ToList();
        }

        public ErrorCode DeleteUser(int userId)
        {
            var user = GetUserById(userId);
            if (user == null)
            {
                return ErrorCode.Error;
            }

            var info = GetVolunteerInfoByUserId(user.userId);
            var skills = GetVolunteerSkillsByUserId(user.userId);
            var joinedEvents = GetJoinedEventByUserId(user.userId);
            var profilePic = GetProfileByUserId(user.userId);
            var userRoles = GetRolesByUserId(user.userId);

            if (info != null)
            {
                var result = _volunteerInfo.Delete(info.volunteerId);
                if (result != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }

            if (joinedEvents != null)
            {
                foreach (var joinedEvent in joinedEvents)
                {
                    var result = _joinedEvent.Delete(joinedEvent.applyVolunteerId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    var result = _volunteerSkill.Delete(skill.volunteerSkillId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            if (profilePic != null)
            {
                foreach (var profile in profilePic)
                {
                    var result = _profilePic.Delete(profile.profileId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }
            if (userRoles != null)
            {
                foreach (var roles in userRoles)
                {
                    var result = _userRoles.Delete(roles.userRoleId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }
            var deleteUserResult = _userAccount.Delete(user.userId);
            if (deleteUserResult != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
    }
}