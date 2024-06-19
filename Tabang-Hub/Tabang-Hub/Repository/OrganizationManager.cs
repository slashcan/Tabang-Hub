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
        public BaseRepository<vw_ListOfEvent> _listOfEvents;
        public BaseRepository<OrgInfo> _orgInfo;
        public BaseRepository<ProfilePicture> _profilePic;
        public BaseRepository<UserDonated> _userDonated;

        public OrganizationManager()
        {
            _orgEvents = new BaseRepository<OrgEvents>();
            _orgSkillRequirements = new BaseRepository<OrgSkillRequirement>();
            _orgEventsImage = new BaseRepository<OrgEventImage>();
            _listOfEvents = new BaseRepository<vw_ListOfEvent>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _userDonated = new BaseRepository<UserDonated>();
        }


        public ErrorCode CreateEvents(OrgEvents orgEvents, List<string> imageFileNames, string[] skills, ref string errMsg)
        {
            // Create the event
            if (orgEvents.eventType == 1)
            {
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
            }
            else
            {
                if (_orgEvents.Create(orgEvents, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }

                // Get the eventId of the newly created event
                int eventId = orgEvents.eventId;              

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

        public List<vw_ListOfEvent> ListOfEvents(int userId, int eventType)
        { 
            return _listOfEvents.GetAll().Where(m => m.User_Id == userId && m.Event_Type == eventType).ToList();
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
    }
}
