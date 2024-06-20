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

        //Tables List
        //public List<UserAccount> userAccounts { get; set; }
        //public List<VolunteerInfo> volunteersInfo { get; set; }
        //public List<VolunteerSkill> volunteersSkill { get; set; }
        //public List<Skills> skills { get; set; }
        //public List<ProfilePicture> picture { get; set; }
        public List<OrgEventImage> detailsEventImage { get; set; }
        public List<OrgSkillRequirement> detailsSkillRequirement { get; set; }
        public List<UserAccount> userAccounts { get; set; }
        public List<VolunteerInfo> volunteersInfo { get; set; }
        public List<VolunteerSkill> volunteersSkills { get; set; }
        public List<Skills> skills { get; set; }
        public List<ProfilePicture> picture { get; set; }
        public List<OrgInfo> orgInfos { get; set; }
        public List<OrgEvents> orgEvents { get; set; }
        public List<Volunteers> volunteers { get; set; }
        public UserDonated userDonated { get; set; }
        public List<UserDonated> listofUserDonated { get; set; }
        public List<Volunteers> listOfEventVolunteers { get; set; }

        //Stored Procedure
        public List<sp_GetSkills_Result> uniqueSkill { get; set; }
        public List<sp_OtherEvent_Result> orgOtherEvent { get; set; }

        //View
        public List<vw_ListOfEvent> listOfEvents { get; set; }
    }
}