using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using Tabang_Hub.Contracts;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class VolunteerManager
    {
        public BaseRepository<Skills> _skills;
        public VolunteerManager() 
        {
            _skills = new BaseRepository<Skills>();
        }

    }
}