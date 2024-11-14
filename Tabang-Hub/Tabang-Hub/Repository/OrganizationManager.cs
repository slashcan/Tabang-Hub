﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Utils;
using static Tabang_Hub.Utils.Lists;

namespace Tabang_Hub.Repository
{
    public class OrganizationManager
    {
        private readonly TabangHubEntities db;

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
        private BaseRepository<VolunteersHistory> _volunteersHistory;
        private BaseRepository<GroupChat> _groupChat;
        private BaseRepository<GroupMessages> _groupMessages;
        private BaseRepository<Rating> _ratings;
        private BaseRepository<vw_VolunteerSkills> _vwVollunterSkills;
        private BaseRepository<VolunteerInfo> _volunteerInfo;
        private BaseRepository<ProfilePicture> _profile;
        private BaseRepository<Notification> _notification;

        public OrganizationManager()
        {
            db = new TabangHubEntities();

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
            _volunteersHistory = new BaseRepository<VolunteersHistory>();
            _groupChat = new BaseRepository<GroupChat>();
            _groupMessages = new BaseRepository<GroupMessages>();
            _ratings = new BaseRepository<Rating>();
            _vwVollunterSkills = new BaseRepository<vw_VolunteerSkills>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _profile = new BaseRepository<ProfilePicture>();
            _notification = new BaseRepository<Notification>();
        }

        public ErrorCode CreateEvents(OrgEvents orgEvents, List<string> imageFileNames, List<string> skills, ref string errMsg)
        {
            // Create the event
            orgEvents.status = 1;
            if (_orgEvents.Create(orgEvents, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            // Get the eventId of the newly created event
            int eventId = orgEvents.eventId;
            int orgInfoId = db.OrgInfo.Where(m => m.userId == orgEvents.userId).Select(m => m.orgInfoId).FirstOrDefault();

            var gc = new GroupChat
            {
                eventId = eventId,
                orgInfoId = orgInfoId
            };
            if (_groupChat.Create(gc, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            // Add each skill associated with the eventId            
            foreach (var skill in skills)
            {
                var skillId = GetSkillIdBySkillName(skill);
                var skillRequirement = new OrgSkillRequirement
                {
                    eventId = eventId,
                    skillId = skillId.skillId,
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


        public ErrorCode EditOrgInfo(OrgInfo orgInformation, int id, ref string errMsg)
        {
            if (orgInformation == null)
            {
                errMsg = "Organization information is required.";
                return ErrorCode.Error;
            }
            var currentOrgInfo = GetOrgInfoByUserId(id);

            // Check if there are changes, otherwise keep the existing values
            currentOrgInfo.userId = id;
            currentOrgInfo.orgName = orgInformation.orgName ?? currentOrgInfo.orgName;
            currentOrgInfo.orgEmail = orgInformation.orgEmail ?? currentOrgInfo.orgEmail;
            currentOrgInfo.orgType = orgInformation.orgType ?? currentOrgInfo.orgType;
            currentOrgInfo.orgDescription = orgInformation.orgDescription ?? currentOrgInfo.orgDescription;
            currentOrgInfo.phoneNum = orgInformation.phoneNum ?? currentOrgInfo.phoneNum;
            currentOrgInfo.street = orgInformation.street ?? currentOrgInfo.street;
            currentOrgInfo.city = orgInformation.city ?? currentOrgInfo.city;
            currentOrgInfo.province = orgInformation.province ?? currentOrgInfo.province;
            currentOrgInfo.profilePath = orgInformation.profilePath ?? currentOrgInfo.profilePath;
            currentOrgInfo.coverPhoto = orgInformation.coverPhoto ?? currentOrgInfo.coverPhoto;

            // Proceed to update if there are changes
            if (_orgInfo.Update(currentOrgInfo.orgInfoId, currentOrgInfo, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode EditEvent(OrgEvents orgEvents, List<string> skills, string[] skillsToRemove, List<string> imageFileNames, int eventId, ref string errMsg)
        {
            // Get the existing skills for the event
            var oldSkills = listOfSkillRequirement(eventId);

            // Step 1: Calculate skillsToActuallyRemove
            if (skillsToRemove != null)
            {
                skillsToRemove = skillsToRemove
                    .SelectMany(s => s.Split(',').Select(skill => skill.Trim()))
                    .Where(skill => !string.IsNullOrEmpty(skill))
                    .Distinct() // Prevent duplicates
                    .ToArray();
            }
            else
            {
                skillsToRemove = new string[0]; // Initialize as an empty array
            }

            var skillsToActuallyRemove = skillsToRemove.Except(skills).ToArray();

            // Step 2: Update existing skills where totalNeeded has changed
            foreach (var oldSkill in oldSkills)
            {
                // Update the skill requirement in the database
                if (_orgSkillRequirements.Update(oldSkill.skillRequirementId, oldSkill, out errMsg) != ErrorCode.Success)
                {
                    // Return an error if the update fails
                    return ErrorCode.Error;
                }
                // Remove the skill from skills dictionary as it's already processed
                skills.Remove(oldSkill.Skills.skillName);
            }

            // Step 3: Create new skills that do not exist in oldSkills
            foreach (var skill in skills)
            {
                // Get the skillId by skill name
                var tempSkill = GetSkillIdBySkillName(skill);

                var newSkillRequirement = new OrgSkillRequirement
                {
                    eventId = eventId,
                    skillId = tempSkill.skillId,
                };

                // Create the new skill requirement entry in the database
                if (_orgSkillRequirements.Create(newSkillRequirement, out errMsg) != ErrorCode.Success)
                {
                    // Return an error if the creation fails
                    return ErrorCode.Error;
                }
            }

            // Step 4: Process skills to actually remove
            var skillsToBeDeleted = new List<int>(); // Collect skill IDs to delete later

            foreach (var skillToRemove in skillsToActuallyRemove)
            {
                // Check if the skill exists in the oldSkills list
                foreach (var oldSkill in oldSkills)
                {
                    if (oldSkill.Skills.skillName == skillToRemove)
                    {
                        // Add the skillRequirementId to the deletion list
                        skillsToBeDeleted.Add(oldSkill.skillRequirementId);
                    }
                }
            }

            // Now, perform the deletions outside the loop to avoid modifying the collection during iteration
            foreach (var skillRequirementId in skillsToBeDeleted)
            {
                // Delete the skill from the database
                if (_orgSkillRequirements.Delete(skillRequirementId) != ErrorCode.Success)
                {
                    // Return an error if the deletion fails
                    return ErrorCode.Error;
                }
            }

            // Step 5: Add new images for the event
            foreach (var fileName in imageFileNames)
            {
                var orgEventImage = new OrgEventImage
                {
                    eventId = eventId,
                    eventImage = fileName
                };

                // Create the new event image entry
                if (_orgEventsImage.Create(orgEventImage, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }

            // Step 6: Ensure that maxVolunteer is calculated by summing up totalNeeded for each skill in the event
            var updatedOldSkills = listOfSkillRequirement(eventId); // Refresh the oldSkills list

            var oldObj = GetEventsByEventId(eventId);

            oldObj.targetAmount = orgEvents.targetAmount; // Can be null or a decimal value

            // Update the rest of the event details (title, description, etc.)
            oldObj.eventTitle = orgEvents.eventTitle;
            oldObj.eventDescription = orgEvents.eventDescription;
            oldObj.maxVolunteer = orgEvents.maxVolunteer;
            oldObj.dateStart = orgEvents.dateStart;
            oldObj.dateEnd = orgEvents.dateEnd;
            oldObj.location = orgEvents.location;

            // Update the event in the database
            if (_orgEvents.Update(eventId, oldObj, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public List<vw_ListOfEvent> ListOfEvents(int userId)
        {
            return _listOfEvents.GetAll()
                                .Where(m => m.User_Id == userId)
                                .OrderByDescending(m => m.Start_Date) // Replace `Event_Date` with the appropriate property for ordering
                                .ToList();
        }

        public List<vw_ListOfEvent> ListOfEvents1(int userId)
        {
            return _listOfEvents.GetAll()
                                .Where(m => m.User_Id == userId && m.Status == 1)
                                .OrderByDescending(m => m.Start_Date) // Replace `Event_Date` with the appropriate property for ordering
                                .ToList();
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

            return totalVolunteer;
        }
        public Dictionary<int, int> GetEventsByUserId(int userId)
        {
            // Events from the main table
            var eventSummary = _orgEvents._table
                .Where(m => m.userId == userId && m.dateStart.HasValue && m.dateEnd.HasValue && m.status == 2)
                .GroupBy(m => m.dateStart.Value.Month)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            // Merge the two dictionaries
            foreach (var month in eventSummary.Keys)
            {
                if (eventSummary.ContainsKey(month))
                {
                    eventSummary[month] += eventSummary[month];
                }
                else
                {
                    eventSummary[month] = eventSummary[month];
                }
            }

            return eventSummary;
        }

        public List<OrgEvents> GetRecentOngoingEventsByUserId(int userId)
        {
            var recentEvents = _orgEvents._table
                .Where(m => m.userId == userId && m.dateStart.HasValue && m.dateEnd.HasValue)
                // Events that have started but not yet ended
                .OrderByDescending(m => m.eventId) // Order by dateStart, most recent first
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
                        var req = listOfSkillRequirement(eventItem.Event_Id);

                        foreach (var require in req)
                        {
                            if (skill.skillId == require.skillId && volunteer.Status == 1)
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
                }
            }

            // Step 4: Return the dictionary containing the skills and their counts
            return skillFrequency;
        }

        public List<Volunteers> GetVolunteersByEventId(int eventId)
        {
            return _eventVolunteers._table.Where(m => m.eventId == eventId).ToList();
        }
        public List<ProfilePicture> GetProfileByUserId(int userId)
        {
            return _profile._table.Where(m => m.userId == userId).ToList();
        }
        public ErrorCode SentNotif(int userId, int senderId, int relatedId, string type, string content, int broadcast, ref string errMsg)
        {
            var notif = new Notification()
            {
                userId = userId,
                senderUserId = senderId,
                relatedId = relatedId,
                type = type,
                content = content,
                broadcast = broadcast,
                status = 0,
                createdAt = DateTime.Now,

            };
            try
            {
                if (_notification.Create(notif, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return ErrorCode.Success;
        }
        public List<VolunteerSkill> GetVolunteerSkillByUserId(int userId)
        {
            return _volunteerSkills._table.Where(m => m.userId == userId).ToList();
        }
        public List<VolunteerSkill> GetVolunteerSkillsByUserId(int? userId)
        {
            return _volunteerSkills._table.Where(m => m.userId == userId).ToList();
        }
        public List<UserDonated> GetTotalDonationByEventId(int eventId)
        {
            return _userDonated._table.Where(m => m.eventId == eventId).ToList();
        }
        public Skills GetSkillIdBySkillName(string skillName)
        {
            return _skills._table.Where(m => m.skillName == skillName).FirstOrDefault();
        }
        public List<Volunteers> GetTotalVolunteerByEventId(int eventId)
        {
            return _eventVolunteers._table.Where(m => m.eventId == eventId).ToList();
        }
        public List<Volunteers> GetPendingVolunteersByEventId(int eventId)
        {
            return _eventVolunteers._table.Where(m => m.eventId == eventId && m.Status == 0).ToList();
        }
        public List<UserAccount> GetListOfVolunteer()
        {
            return _userAccount._table.Where(m => m.roleId == 1 && m.status == 1).ToList();
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
        public OrgInfo GetOrgInfoByUserId(int id)
        {
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public OrgEvents GetEventById(int id)
        {
            return _orgEvents._table.Where(m => m.eventId == id).FirstOrDefault();
        }
        public OrgEvents GetEventsByEventId(int id)
        {
            return _orgEvents._table.Where(m => m.eventId == id).FirstOrDefault();
        }
        public OrgEventImage GetEventImageByEventId(int eventId)
        {
            return _orgEventsImage._table.Where(m => m.eventId == eventId).FirstOrDefault();
        }
        public List<OrgEvents> GetOrgEventsByUserId(int userId)
        {
            return _orgEvents._table.Where(m => m.userId == userId).ToList();
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
        public List<Rating> GetVolunteerRatingsByUserId(int userId)
        {
            return _ratings._table.Where(m => m.userId == userId).ToList();
        }
        public List<VolunteerSkill> GetListOfVolunteerSkillByUserId(int userId)
        {
            return _volunteerSkills.GetAll().Where(m => m.userId == userId).ToList();
        }
        public UserAccount GetUserByUserId(int userId)
        {
            return _userAccount._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public VolunteerInfo GetVolunteerInfoByUserId(int userId)
        {
            return _volunteerInfo._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public List<OrgEvents> GetEventHistoryByUserId(int userId)
        {
            return _orgEvents._table.Where(m => m.userId == userId && m.status == 2).ToList();
        }

        public GroupChat GetGroupChatByEventId(int eventId)
        {
            return _groupChat._table.Where(m => m.eventId == eventId).FirstOrDefault();
        }
        public List<GroupMessages> GetGroupMessagesByGroupChatId(int groupChatId)
        {
            return _groupMessages._table.Where(m => m.groupChatId == groupChatId).ToList();
        }
        public Volunteers GetSkillIdByEventIdAndUserId(int eventId, int userId)
        {
            return _eventVolunteers._table.Where(m => m.userId == userId && m.eventId == eventId).FirstOrDefault();
        }
        public OrgEvents GetEventByEventId(int eventId)
        {
            return _orgEvents._table.Where(m => m.eventId == eventId).FirstOrDefault();
        }
        public ErrorCode DeleteEvent(int eventId)
        {
            var skillsRequirement = listOfSkillRequirement(eventId);
            var eventImage = listOfEventImage(eventId);
            var eventVolunteers = ListOfEventVolunteers(eventId);
            var userDonated = ListOfUserDonated(eventId);
            var groupChat = GetGroupChatByEventId(eventId);


            if (groupChat != null)
            {
                var groupMessage = GetGroupMessagesByGroupChatId(groupChat.groupChatId);

                foreach (var message in groupMessage)
                {
                    if (_groupMessages.Delete(message.messageId) != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }

                if (_groupChat.Delete(groupChat.groupChatId) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }

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
        public ErrorCode DeclineApplicant(int id, int eventId)
        {
            var user = GetVolunteerById(id, eventId);


            if (_eventVolunteers.Delete(user.applyVolunteerId) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
       
        public async Task<List<FilteredVolunteer>> GetMatchedVolunteers(int eventId)
        {
            string flaskApiUrl = "http://127.0.0.1:5000/recruit"; // Flask API URL
            List<FilteredVolunteer> recruit = new List<FilteredVolunteer>();
            string errorMessage = null;


            // Get the target event's date range
            var targetEvent = _orgEvents.GetAll().FirstOrDefault(m => m.eventId == eventId);
            if (targetEvent == null)
            {
                throw new Exception("Event not found");
            }
            DateTime targetStartDate = targetEvent.dateStart ?? DateTime.MinValue;
            DateTime targetEndDate = targetEvent.dateEnd ?? DateTime.MaxValue;

            // Get volunteers who do not have conflicting events
            var availableVolunteers = _vwVollunterSkills.GetAll()
                .Where(vol => !db.Volunteers.ToList().Any(e =>
                    e.userId == vol.userId &&
                    (e.Status == 0 || e.Status == 1) && // Check for pending or ongoing events
                    _orgEvents.GetAll().Any(ev =>
                        ev.eventId == e.eventId &&
                        (ev.dateStart <= targetEndDate && ev.dateEnd >= targetStartDate) // Exclude if date overlap
                    )
                ))
                .ToList();

            // Prepare the data to pass to Flask
            var datas = new
            {
                // Retrieve only relevant user skills for volunteers
                user_skills = availableVolunteers.Select(m => new { userId = m.userId, skillId = m.skillId, rating = m.rate }).ToList(),

                // Pass only the specific event's data
                event_data = _orgEvents.GetAll().Where(m => m.eventId == eventId).Select(m => new { eventId = m.eventId, eventDescription = m.eventDescription }).ToList(),

                // Only pass skills required for the specific event
                event_skills = db.OrgSkillRequirement.Where(es => es.eventId == eventId).Select(es => new { eventId = es.eventId, skillId = es.skillId }).ToList(),

                // Include volunteer history for future use in model training
                volunteer_history = _volunteersHistory.GetAll().Select(vh => new { eventId = vh.eventId, attended = vh.attended }).ToList()
            };

            using (var client = new HttpClient())
            {
                try
                {
                    // Step 1: Send POST request to Flask API with the requestData
                    var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                    if (response.IsSuccessStatusCode)
                    {
                        // Step 2: Deserialize Flask API response to a list of recommended events
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        recruit = JsonConvert.DeserializeObject<List<FilteredVolunteer>>(jsonResponse);
                    }
                    else
                    {
                        errorMessage = "Error calling the Python API: " + response.ReasonPhrase;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = "An error occurred: " + ex.Message;
                }
            }

            return recruit; // Return the list of recommended volunteers and any error message
        }

        public ErrorCode TrasferToHisotry1(int eventId, List<VolunteerRatingData> volunteerRatings, ref string errMsg)
        {
            var orgEvent = GetEventByEventId(eventId);
            var volunteers = ListOfEventVolunteers(eventId);

            foreach (var volunteer in volunteers)
            {
                foreach (var rate in volunteerRatings)
                {
                    if (volunteer.userId == rate.VolunteerId)
                    {
                        var volunteersHistory = new VolunteersHistory()
                        {

                            userId = volunteer?.userId,
                            eventId = volunteer.eventId,
                            appliedAt = volunteer?.appliedAt,
                            attended = rate.Attendance,
                        };

                        if (_volunteersHistory.Create(volunteersHistory, out errMsg) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                    }
                }
               
            }       

            orgEvent.status = 2;

            if (_orgEvents.Update(orgEvent.eventId, orgEvent, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public ErrorCode SaveRating(int eventId, int attended, int userId, int skillId, int rating, ref string errMsg)
        {
            var skillId1 = GetSkillIdByEventIdAndUserId(eventId, userId);
            var ratings = new Rating()
            {
                eventId = eventId,
                userId = userId,
                rate = rating,
                skillId = skillId,
                ratedAt = DateTime.Now,
            };

            if (_eventVolunteers.Update(skillId1.applyVolunteerId, skillId1, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            if (_ratings.Create(ratings, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode InviteVolunteer(int userId, int eventId, ref string errMsg)
        {
            var volunteer = new Volunteers()
            {
                userId = userId,
                eventId = eventId,
                Status = 2,
            };

            if (_eventVolunteers.Create(volunteer, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
    }
}