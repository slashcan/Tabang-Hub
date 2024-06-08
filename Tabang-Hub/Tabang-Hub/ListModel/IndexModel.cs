using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.ListModel
{
    public class IndexModel
    {
        //Tables
        public List<UserAccount> userAccounts { get; set; }
        public List<VolunteerInfo> volunteersInfo { get; set; }
        public List<VolunteerSkill> volunteersSkill { get; set; }
        public List<Skills> skills { get; set; }


        //Stored Procedure
        public List<sp_GetSkills_Result> uniqueSkill { get; set; }
    }
}