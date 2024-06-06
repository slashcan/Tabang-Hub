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

        public OrganizationManager()
        {
            _orgEvents = new BaseRepository<OrgEvents>();
            _orgSkillRequirements = new BaseRepository<OrgSkillRequirement>();
            _orgEventsImage = new BaseRepository<OrgEventImage>();
            _listOfEvents = new BaseRepository<vw_ListOfEvent>();
        }


        public ErrorCode CreateEvents(OrgEvents orgEvents, List<string> imageFileNames, string[] skills, ref string errMsg)
        {
            // Create the event
            if (_orgEvents.Create(orgEvents, out errMsg) != Contracts.ErrorCode.Success)
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

                if (_orgSkillRequirements.Create(skillRequirement, out errMsg) != Contracts.ErrorCode.Success)
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

                if (_orgEventsImage.Create(orgEventImage, out errMsg) != Contracts.ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }

            return ErrorCode.Success;
        }

        public List<vw_ListOfEvent> ListOfEvents()
        { 
            return _listOfEvents.GetAll();
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
    }
}
