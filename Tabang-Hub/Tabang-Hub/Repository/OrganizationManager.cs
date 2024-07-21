using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class OrganizationManager
    {
        private BaseRepository<OrgEvents> _orgEvents;
        private BaseRepository<OrgSkillRequirement> _orgSkillRequirements;
        private BaseRepository<OrgEventImage> _orgEventsImage;
        private BaseRepository<vw_ListOfEvent> _listOfEvents;
        private BaseRepository<OrgInfo> _orgInfo;
        private BaseRepository<ProfilePicture> _profilePic;
        private BaseRepository<UserDonated> _userDonated;
        private BaseRepository<Volunteers> _eventVolunteers;
        private BaseRepository<VolunteerSkill> _volunteerSkills;
        private BaseRepository<UserAccount> _userAccount;

        public OrganizationManager()
        {
            _orgEvents = new BaseRepository<OrgEvents>();
            _orgSkillRequirements = new BaseRepository<OrgSkillRequirement>();
            _orgEventsImage = new BaseRepository<OrgEventImage>();
            _listOfEvents = new BaseRepository<vw_ListOfEvent>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _userDonated = new BaseRepository<UserDonated>();
            _eventVolunteers = new BaseRepository<Volunteers>();
            _volunteerSkills = new BaseRepository<VolunteerSkill>();
            _userAccount = new BaseRepository<UserAccount>();
        }


        public ErrorCode CreateEvents(OrgEvents orgEvents, List<string> imageFileNames, string[] skills, ref string errMsg)
        {
            // Create the event
            if (_orgEvents.Create(orgEvents, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            // Get the eventId of the newly created event
            int eventId = orgEvents.eventId;

            // Add each skill associated with the eventId            
            foreach (var skill in skills)
            {
                var skillRequirement = new OrgSkillRequirement
                {
                    eventId = eventId,
                    skillName = skill
                };

                if (_orgSkillRequirements.Create(skillRequirement, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }

            // Add each image associated with the eventId
            foreach (var fileName in imageFileNames)
            {
                var orgEventImage = new OrgEventImage
                {
                    eventId = eventId,
                    eventImage = fileName
                };

                if (_orgEventsImage.Create(orgEventImage, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            return ErrorCode.Success;
        }

        public ErrorCode EditOrgInfo(OrgInfo orgInformation, ProfilePicture profilePic, int id, ref string errMsg)
        {
            if (orgInformation == null)
            {
                errMsg = "Organization information is required.";
                return ErrorCode.Error;
            }

            if (profilePic == null)
            {
                errMsg = "Profile picture is required.";
                return ErrorCode.Error;
            }

            profilePic.userId = orgInformation.userId;
            if (_profilePic.Create(profilePic, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            orgInformation.profileId = profilePic.profileId;
            var orgId = GetOrgInfoByUserId(id);

            orgId.userId = id;
            orgId.orgName = orgInformation.orgName;
            orgId.orgEmail = orgInformation.orgEmail;
            orgId.orgType = orgInformation.orgType;
            orgId.orgDescription = orgInformation.orgDescription;
            orgId.phoneNum = orgInformation.phoneNum;
            orgId.street = orgInformation.street;
            orgId.city = orgInformation.city;
            orgId.province = orgInformation.province;
            orgId.profileId = profilePic.profileId;

            if (_orgInfo.Update(orgId.orgInfoId, orgId, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }

        public List<vw_ListOfEvent> ListOfEvents(int userId)
        {
            return _listOfEvents.GetAll().Where(m => m.User_Id == userId).ToList();
        }
        public Volunteers GetVolunteerById(int id)
        {
            return _eventVolunteers._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public OrgInfo GetOrgInfoByUserId(int? id)
        {
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public OrgInfo GetOrgInfoByUserId(int id)
        {
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public vw_ListOfEvent GetEventById(int id)
        {
            return _listOfEvents._table.Where(m => m.Event_Id == id).FirstOrDefault();
        }

        public List<OrgEventImage> listOfEventImage(int id)
        {
            return _orgEventsImage.GetAll().Where(m => m.eventId == id).ToList();
        }

        public List<OrgSkillRequirement> listOfSkillRequirement(int id)
        {
            return _orgSkillRequirements.GetAll().Where(m => m.eventId == id).ToList();
        }
        public List<UserDonated> ListOfUserDonated(int id)
        {
            return _userDonated.GetAll().Where(m => m.eventId == id).ToList();
        }
        public List<Volunteers> ListOfEventVolunteers(int eventId)
        {
            return _eventVolunteers.GetAll().Where(m => m.eventId == eventId).ToList();
        }
        public List<VolunteerSkill> ListOfEventVolunteerSkills()
        {
            return _volunteerSkills.GetAll();
        }
        public UserAccount GetUserByUserId(int userId)
        {
            return _userAccount._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public ErrorCode DeleteEvent(int eventId)
        {
            var skillsRequirement = listOfSkillRequirement(eventId);
            var eventImage = listOfEventImage(eventId);
            var eventVolunteers = ListOfEventVolunteers(eventId);
            var userDonated = ListOfUserDonated(eventId);

            if (skillsRequirement != null)
            {
                foreach (var skillRequirement in skillsRequirement)
                {
                    var result = _orgSkillRequirements.Delete(skillRequirement.skillRequirementId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            if (eventImage != null)
            {
                foreach (var eventImages in eventImage)
                {
                    var result = _orgEventsImage.Delete(eventImages.eventImageId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }
            if (userDonated != null)
            {
                foreach (var userDonate in userDonated)
                {
                    var result = _userDonated.Delete(userDonate.orgUserDonatedId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            if (eventVolunteers != null)
            {
                foreach (var eventVolunteer in eventVolunteers)
                {
                    var result = _eventVolunteers.Delete(eventVolunteer.applyVolunteerId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            var deleteEventResult = _orgEvents.Delete(eventId);
            if (deleteEventResult != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode ConfirmApplicants(int id, ref string errMsg)
        {
            var user = GetVolunteerById(id);

            user.Status = 1;

            if (_eventVolunteers.Update(user.applyVolunteerId, user, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
    }
}