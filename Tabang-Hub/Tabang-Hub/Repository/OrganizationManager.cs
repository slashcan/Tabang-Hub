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

        public OrganizationManager()
        {
            _orgEvents = new BaseRepository<OrgEvents>();
            _orgSkillRequirements = new BaseRepository<OrgSkillRequirement>();
            _orgEventsImage = new BaseRepository<OrgEventImage>();
        }

        public ErrorCode CreateEvents(OrgEvents orgEvents, OrgEventImage orgEventImage, string[] skills, ref String errMsg)
        {
            // Create the event
            if (_orgEvents.Create(orgEvents, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            // Get the eventId of the newly created event
            int eventId = orgEvents.eventId;
            orgEventImage.eventId = eventId;

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

            if (_orgEventsImage.Create(orgEventImage, out errMsg) != Contracts.ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
    }
}
