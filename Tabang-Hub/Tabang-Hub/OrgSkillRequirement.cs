//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tabang_Hub
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrgSkillRequirement
    {
        public int skillRequirementId { get; set; }
        public Nullable<int> eventId { get; set; }
        public Nullable<int> skillId { get; set; }
        public Nullable<int> totalNeeded { get; set; }
    
        public virtual OrgEvents OrgEvents { get; set; }
        public virtual Skills Skills { get; set; }
    }
}
