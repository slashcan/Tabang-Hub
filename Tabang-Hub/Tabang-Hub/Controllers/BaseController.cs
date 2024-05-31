using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Repository;

namespace Tabang_Hub.Controllers
{
    public class BaseController : Controller
    {
        public BaseRepository<UserAccount> _userAcc;
        public TabangHubEntities _db;
        public BaseController()
        {
            _db = new TabangHubEntities();
            _userAcc = new BaseRepository<UserAccount>();
        }
    }
}