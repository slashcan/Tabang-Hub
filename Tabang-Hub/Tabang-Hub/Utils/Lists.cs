using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class Lists
    {
        public OrgEvents CreateEvents {  get; set; }
        public OrgSkillRequirement skillRequirement { get; set; }
        
        public vw_ListOfEvent eventDetails { get; set; }
        
        public UserAccount userAccount { get; set; }
        public OrgInfo OrgInfo { get; set; }
        public ProfilePicture profilePic { get; set; }

<<<<<<< Updated upstream
        //Tables
=======
        //Tables List
        public List<OrgEventImage> detailsEventImage { get; set; }
        public List<OrgSkillRequirement> detailsSkillRequirement { get; set; }
>>>>>>> Stashed changes
        public List<UserAccount> userAccounts { get; set; }
        public List<VolunteerInfo> volunteersInfo { get; set; }
        public List<VolunteerSkill> volunteersSkill { get; set; }
        public List<Skills> skills { get; set; }
<<<<<<< Updated upstream

=======
        public List<ProfilePicture> picture { get; set; }
        public List<OrgInfo> orgInfos { get; set; }
        public List<OrgEvents> orgEvents { get; set; }
>>>>>>> Stashed changes

        //Stored Procedure
        public List<sp_GetSkills_Result> uniqueSkill { get; set; }

        //View
        public List<vw_ListOfEvent> listOfEvents { get; set; }
    }
}