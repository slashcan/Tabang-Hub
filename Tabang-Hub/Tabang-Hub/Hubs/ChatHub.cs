using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Tabang_Hub.Utils;
using System.Web.Mvc;
using System.Web.Helpers;
using Tabang_Hub.Repository;
using System.Data.Entity;
using Tabang_Hub.Controllers;
using System.Threading.Tasks;

namespace Tabang_Hub.Hubs
{
    public class ChatHub : Hub
    {
        private readonly TabangHubEntities _db;

        public ChatHub()
        {
            _db = new TabangHubEntities();
        }
        public void Send(int userId, int groupId, string message)
        {
            if(userId.Equals(null) || groupId.Equals(null) || string.IsNullOrEmpty(message))
            {
                return;
            }

            var userInfo = _db.VolunteerInfo.Where(m => m.userId == userId).FirstOrDefault();
            var userName = userInfo != null ? char.ToUpper(userInfo.fName[0]) + userInfo.fName.Substring(1) + ' ' + char.ToUpper(userInfo.lName[0]) + userInfo.lName.Substring(1) : "Unknown User";

            var gc = new GroupMessages
            {
                message = message,
                messageAt = DateTime.Now,
                groupChatId = groupId,
                userId = userId,
            };

            _db.GroupMessages.Add(gc);
            _db.SaveChanges();

            Clients.All.broadcastMessage(userName, userId, message, groupId);
        }

        public void GetAllMessages(int groupId)
        {
            var messages = _db.sp_GetAllMessage(groupId);
            Clients.Caller.loadMessages(messages);
        }
    }
}