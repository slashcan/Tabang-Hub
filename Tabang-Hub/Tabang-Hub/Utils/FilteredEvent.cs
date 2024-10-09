using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class FilteredEvent
    {
        public int EventID { get; set; }
        public string EventDescription { get; set; }
        public string RequiredSkills { get; set; }
        public string MatchedSkills { get; set; }
    }
}