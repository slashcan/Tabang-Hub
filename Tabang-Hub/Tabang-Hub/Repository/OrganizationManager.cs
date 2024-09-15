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
        private BaseRepository<Skills> _skills;
        private BaseRepository<OrgEventHistory> _orgEventHistory;
        private BaseRepository<OrgSkillRequirementsHistory> _skillRequirementsHistory;
        private BaseRepository<UserDonatedHistory> _userDonatedHistory;
        private BaseRepository<VolunteersHistory> _volunteersHistory;
        private BaseRepository<OrgEventImageHistory> _orgEventImageHistory;
        private BaseRepository<VolunteerSkillsHistory> _volunteerSkillHistory;

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
            _skills = new BaseRepository<Skills>();
            _orgEventHistory = new BaseRepository<OrgEventHistory>();
            _skillRequirementsHistory = new BaseRepository<OrgSkillRequirementsHistory>();
            _userDonatedHistory = new BaseRepository<UserDonatedHistory>();
            _volunteersHistory = new BaseRepository<VolunteersHistory>();
            _orgEventImageHistory = new BaseRepository<OrgEventImageHistory>();
            _volunteerSkillHistory = new BaseRepository<VolunteerSkillsHistory>();
        }


        public ErrorCode CreateEvents(OrgEvents orgEvents, List<string> imageFileNames, Dictionary<string, int> skills, ref string errMsg)
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
                var skillId = GetSkillIdBySkillName(skill.Key);
                var skillRequirement = new OrgSkillRequirement
                {
                    eventId = eventId,
                    skillId = skillId.skillId,
                    totalNeeded = skill.Value
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

            orgInformation.profilePath = profilePic.profilePath;
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
            orgId.profilePath = profilePic.profilePath;

            if (_orgInfo.Update(orgId.orgInfoId, orgId, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }

        public ErrorCode EditEvent(OrgEvents orgEvents, string[] skills, List<string> imageFileNames, int eventId, ref string errMsg)
        {
            var oldSkills = listOfSkillRequirement(eventId);

            // Create a dictionary of existing skill names for quick lookup
            var oldSkillDict = oldSkills.ToDictionary(s => s.Skills.skillName, s => s.skillRequirementId);

            var skillsToUpdate = new HashSet<string>(skills);

            // Update or delete old skills
            foreach (var oldSkill in oldSkills)
            {
                if (skillsToUpdate.Contains(oldSkill.skillId.ToString()))
                {
                    // Skill exists in the new list, so we update it (if needed)
                    var updatedSkill = new OrgSkillRequirement()
                    {
                        skillId = oldSkill.skillId
                    };
                    _orgSkillRequirements.Update(oldSkill.skillRequirementId, updatedSkill, out errMsg);
                    skillsToUpdate.Remove(oldSkill.ToString()); // Remove it from the update list
                }
                else
                {
                    // Skill no longer exists in the new list, so we delete it
                    _orgSkillRequirements.Delete(oldSkill.skillRequirementId);
                }
            }

            // Create new skills that are not already in the old skills
            foreach (var newSkill in skillsToUpdate)
            {
                var newSkillRequirement = new OrgSkillRequirement()
                {
                    eventId = eventId,
                    //skillName = newSkill
                };
                _orgSkillRequirements.Create(newSkillRequirement, out errMsg);
            }

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

            var oldObj = GetEventsById(eventId);
            oldObj.eventTitle = orgEvents.eventTitle;
            oldObj.eventDescription = orgEvents.eventDescription;
            oldObj.targetAmount = orgEvents.targetAmount;
            oldObj.maxVolunteer = orgEvents.maxVolunteer;
            oldObj.dateStart = orgEvents.dateStart;
            oldObj.dateEnd = orgEvents.dateEnd;
            oldObj.location = orgEvents.location;

            if (_orgEvents.Update(eventId, oldObj, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }

        public List<vw_ListOfEvent> ListOfEvents(int userId)
        {
            return _listOfEvents.GetAll().Where(m => m.User_Id == userId).ToList();
        }
       
        public decimal GetTotalDonationByUserId(int userId) 
        {
            var events = ListOfEvents(userId);

            decimal? totalDonation = 0;

            foreach (var totalEvent in events)
            {
                var donations = GetTotalDonationByEventId(totalEvent.Event_Id);

                foreach (var donation in donations)
                {
                    totalDonation += donation.amount;
                }
            }

            var historyEvents = GetEventHistoryByUserId(userId); // Assuming this retrieves the historical events
            foreach (var historyEvent in historyEvents)
            {
                var donationsHistory = GetTotalDonationHistoryByEventId((int)historyEvent.eventId);
                foreach (var donation in donationsHistory)
                {
                    totalDonation += donation.amount;
                }
            }

            return (decimal)totalDonation;
        }
        public int GetTotalVolunteerByUserId(int userId)
        {
            var events = ListOfEvents(userId);

            int totalVolunteer = 0;

            foreach (var totalEvent in events)
            { 
                var volunteers = GetTotalVolunteerByEventId(totalEvent.Event_Id);

                totalVolunteer += volunteers.Count();
         
            }

            var historyEvents = GetEventHistoryByUserId(userId);
            foreach (var historyEvent in historyEvents)
            {
                var volunteersHistory = GetTotalVolunteerHistoryByEventId((int)historyEvent.eventId);
                totalVolunteer += volunteersHistory.Count();
            }

            return totalVolunteer;
        }
        public Dictionary<int, int> GetEventsByUserId(int userId)
        {
            // Events from the main table
            var eventSummary = _orgEvents._table
                .Where(m => m.userId == userId && m.dateStart.HasValue && m.dateEnd.HasValue && m.dateEnd.Value <= DateTime.Now)
                .GroupBy(m => m.dateStart.Value.Month)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            // Events from the history table
            var historySummary = _orgEventHistory._table
                .Where(m => m.userId == userId)
                .GroupBy(m => m.dateStart.Value.Month)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            // Merge the two dictionaries
            foreach (var month in historySummary.Keys)
            {
                if (eventSummary.ContainsKey(month))
                {
                    eventSummary[month] += historySummary[month];
                }
                else
                {
                    eventSummary[month] = historySummary[month];
                }
            }

            return eventSummary;
        }

        public List<OrgEvents> GetRecentOngoingEventsByUserId(int userId)
        {
            var recentEvents = _orgEvents._table
                .Where(m => m.userId == userId && m.dateStart.HasValue && m.dateEnd.HasValue &&
                            m.dateStart.Value <= DateTime.Now && m.dateEnd.Value > DateTime.Now)
                // Events that have started but not yet ended
                .OrderByDescending(m => m.dateStart.Value) // Order by dateStart, most recent first
                .Take(5) // Get the top 5 most recent events
                .ToList();

            return recentEvents;
        }
        public List<UserDonated> GetRecentUserDonationsByUserId(int userId)
        {
            // Step 1: Get the list of events created by the organization
            var events = ListOfEvents(userId); // Assuming ListOfEvents(userId) returns a list of events for the user

            // Step 2: Extract the event IDs from the event list
            var eventIds = events.Select(e => e.Event_Id).ToList();

            // Step 3: Get donations related to those event IDs, sorted by the most recent donation date
            var recentDonations = _userDonated._table
                .Where(d => eventIds.Contains((int)d.eventId)) // This will work if eventIds is a list of integers
                .OrderByDescending(d => d.donatedAt) // Sort by donation date
                .Take(5) // Get the top 5 most recent donations
                .ToList();

            return recentDonations;
        }
        public Dictionary<string, int> GetAllVolunteerSkills(int userId)
        {
            // Step 1: Get the list of events created by the user (Organization or Event Creator)
            var events = ListOfEvents(userId); // Assuming ListOfEvents(userId) returns a list of events
            var eventsHstory = GetEventHistoryByUserId(userId);
            // Step 2: Initialize a dictionary to count the frequency of each skill by name
            Dictionary<string, int> skillFrequency = new Dictionary<string, int>();

            // Step 3: For each event, retrieve the volunteers and their associated skills
            foreach (var eventItem in events)
            {
                // Assuming you have a method GetVolunteersByEventId that retrieves all volunteers for an event
                var volunteers = GetVolunteersByEventId(eventItem.Event_Id);

                // For each volunteer, get their skills
                foreach (var volunteer in volunteers)
                {
                    // Assuming you have a method GetVolunteerSkillsByUserId to get the skills of a volunteer
                    var skills = GetVolunteerSkillsByUserId(volunteer.userId);

                    // Count the occurrence of each skill by its name
                    foreach (var skill in skills)
                    {
                        if (skillFrequency.ContainsKey(skill.Skills.skillName)) // Assuming SkillName is a string representing the skill's name
                        {
                            skillFrequency[skill.Skills.skillName]++; // Increment count if skill already exists
                        }
                        else
                        {
                            skillFrequency[skill.Skills.skillName] = 1; // Initialize with count 1 if it doesn't exist
                        }
                    }
                }
            }

            foreach (var eventItem in eventsHstory)
            {
                // Assuming you have a method GetVolunteersByEventId that retrieves all volunteers for an event
                var volunteers = GetTotalVolunteerHistoryByEventId((int)eventItem.eventId);

                // For each volunteer, get their skills
                foreach (var volunteer in volunteers)
                {
                    // Assuming you have a method GetVolunteerSkillsByUserId to get the skills of a volunteer
                    var skills = GetVolunteerSkillHistoryByUserId((int)volunteer.userId);

                    // Count the occurrence of each skill by its name
                    foreach (var skill in skills)
                    {
                        if (skillFrequency.ContainsKey(skill.Skills.skillName)) // Assuming SkillName is a string representing the skill's name
                        {
                            skillFrequency[skill.Skills.skillName]++; // Increment count if skill already exists
                        }
                        else
                        {
                            skillFrequency[skill.Skills.skillName] = 1; // Initialize with count 1 if it doesn't exist
                        }
                    }
                }
            }

            // Step 4: Return the dictionary containing the skills and their counts
            return skillFrequency;
        }

        public List<Volunteers> GetVolunteersByEventId(int eventId)
        { 
            return _eventVolunteers._table.Where(m => m.eventId == eventId).ToList();
        }
        public List<VolunteerSkill> GetVolunteerSkillsByUserId(int? userId)
        {
            return _volunteerSkills._table.Where(m => m.userId == userId).ToList();
        }
        public List<VolunteerSkillsHistory> GetVolunteerSkillHistoryByUserId(int userId)
        { 
            return _volunteerSkillHistory._table.Where(m => m.userId == userId).ToList();
        }
        public List<UserDonated> GetTotalDonationByEventId(int eventId)
        { 
            return _userDonated._table.Where(m => m.eventId == eventId).ToList();
        }
        public List<UserDonatedHistory> GetTotalDonationHistoryByEventId(int eventId)
        { 
            return _userDonatedHistory._table.Where(d => d.eventId == eventId).ToList();
        }
        public List<Volunteers> GetTotalVolunteerByEventId(int eventId)
        {
            return _eventVolunteers._table.Where(m => m.eventId == eventId).ToList();
        }
        public List<VolunteersHistory> GetTotalVolunteerHistoryByEventId(int eventId)
        { 
            return _volunteersHistory._table.Where(v => v.eventId == eventId).ToList();
        }
        public OrgInfo GetProfileByProfileId(int? id)
        { 
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public List<Skills> ListOfSkills()
        { 
            return _skills.GetAll().ToList();
        }
        public Volunteers GetVolunteerById(int id, int eventId)
        {
            return _eventVolunteers._table.Where(m => m.userId == id && m.eventId == eventId).FirstOrDefault();
        }
        public OrgInfo GetOrgInfoByUserId(int? id)
        {
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public Skills GetSkillIdBySkillName(string name)
        { 
            return _skills._table.Where(m => m.skillName == name).FirstOrDefault();
        }
        public OrgInfo GetOrgInfoByUserId(int id)
        {
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public vw_ListOfEvent GetEventById(int id)
        {
            return _listOfEvents._table.Where(m => m.Event_Id == id).FirstOrDefault();
        }
        public OrgEvents GetEventsById(int id)
        {
            return _orgEvents._table.Where(m => m.eventId == id).FirstOrDefault();
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
        public List<VolunteerSkill> GetListOfVolunteerSkillByUserId(int userId)
        { 
            return _volunteerSkills.GetAll().Where(m => m.userId == userId).ToList();
        }
        public UserAccount GetUserByUserId(int userId)
        {
            return _userAccount._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public List<OrgEventHistory> GetEventHistoryByUserId(int userId)
        {
            return _orgEventHistory.GetAll().Where(m => m.userId == userId).ToList();
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
        public ErrorCode ConfirmApplicants(int id, int eventId, ref string errMsg)
        {
            var user = GetVolunteerById(id, eventId);

            user.Status = 1;

            if (_eventVolunteers.Update(user.applyVolunteerId, user, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode TransferToHistory(int userId, ref string errMsg)
        {
            var toTransfer = ListOfEvents(userId);

            foreach (var transfer in toTransfer)
            {
                var userDonate = ListOfUserDonated(transfer.Event_Id);
                var volunteers = ListOfEventVolunteers(transfer.Event_Id);
                var skillRequirements = listOfSkillRequirement(transfer.Event_Id);
                var orgImage = listOfEventImage(transfer.Event_Id);

                if (transfer.End_Date <= DateTime.Now)
                {
                    foreach (var donated in userDonate)
                    {
                        var donateHistory = new UserDonatedHistory()
                        {
                            eventId = (int)donated.eventId,
                            userId = (int)donated.userId,
                            amount = donated.amount,
                        };
                        if (_userDonatedHistory.Create(donateHistory, out errMsg) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }

                        if (_userDonated.Delete(donated.orgUserDonatedId) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                    }
                    foreach (var volunteer in volunteers)
                    {
                        var volunteerSkills = GetListOfVolunteerSkillByUserId((int)volunteer.skillId);

                        foreach (var skills in volunteerSkills)
                        {
                            var volunteerSkillHistory = new VolunteerSkillsHistory()
                            { 
                                userId = skills.userId,
                                skillId = skills.skillId,
                            };

                            if (_volunteerSkillHistory.Create(volunteerSkillHistory, out errMsg) != ErrorCode.Success)
                            {
                                return ErrorCode.Error;
                            }
                            if (_volunteerSkills.Delete(skills.volunteerSkillId) != ErrorCode.Success)
                            {
                                return ErrorCode.Error;
                            }
                        }

                        var volunteersHistory = new VolunteersHistory()
                        {
                            eventId = volunteer.eventId,
                            userId = volunteer?.userId,
                            appliedAt = volunteer?.appliedAt,
                            skillId = volunteer.skillId,
                            Status = volunteer?.Status,
                        };                        

                        if (_volunteersHistory.Create(volunteersHistory, out errMsg) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                        if (_eventVolunteers.Delete(volunteer.applyVolunteerId) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                    }
                    foreach (var skillRequire in skillRequirements)
                    {
                        var skillRequirementsHistory = new OrgSkillRequirementsHistory()
                        {
                            eventId = (int)skillRequire.eventId,
                            skillId = skillRequire.skillId,
                            totalNeeded = skillRequire.totalNeeded,
                        };
                        if (_skillRequirementsHistory.Create(skillRequirementsHistory, out errMsg) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                        if (_orgSkillRequirements.Delete(skillRequire.skillRequirementId) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                    }
                    foreach (var image in orgImage)
                    {
                        var imageHistory = new OrgEventImageHistory()
                        {
                            eventId = (int)image.eventId,
                            eventImage = image.eventImage,
                        };
                        if (_orgEventImageHistory.Create(imageHistory, out errMsg) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                        if (_orgEventsImage.Delete(image.eventImageId) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                    }

                    var eventHistory = new OrgEventHistory()
                    {
                        eventId = transfer.Event_Id,
                        userId = (int)transfer.User_Id,
                        eventTitle = transfer.Event_Name,
                        eventDescription = transfer.Description,
                        targetAmount = transfer.Target_Amount,
                        maxVolunteer = transfer.Max_Volunteer,
                        dateStart = transfer.Start_Date,
                        dateEnd = transfer.End_Date,
                        location = transfer.Location,
                    };
                    if (_orgEventHistory.Create(eventHistory, out errMsg) != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                    if (_orgEvents.Delete(transfer.Event_Id) != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }
            return ErrorCode.Success;
        }
    }
}