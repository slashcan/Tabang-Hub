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
        public BaseRepository<UserDonated> _userDonated;
        public BaseRepository<Volunteers> _volunteers;
        public BaseRepository<vw_ListOfEvent> _vw_listOfEvent;
        public BaseRepository<VolunteersHistory> _volunteersHistory;

        public TabangHubEntities db = new TabangHubEntities();
        public OrganizationManager OrganizationManager;
        public VolunteerManager() 
        {
            _skills = new BaseRepository<Skills>();
            _userDonated = new BaseRepository<UserDonated>();
            _volunteers = new BaseRepository<Volunteers>();
            _vw_listOfEvent = new BaseRepository<vw_ListOfEvent>();

            OrganizationManager = new OrganizationManager();
            db = new TabangHubEntities();
        }

        public ErrorCode CreateDonation(UserDonated userDonated, ref String errMsg)
        {
            if (_userDonated.Create(userDonated, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public Volunteers GetVolunteerByUserId(int userId, int eventId)
        {
            return _volunteers._table.Where(m => m.userId == userId && m.eventId == eventId).FirstOrDefault();
        }
        public List<Volunteers> GetVolunteersEventPendingByUserId(int userId)
        {
            var pendingEvents = _volunteers.GetAll().Where(m => m.userId == userId && m.Status == 0).ToList();
            List<Volunteers> pendings = new List<Volunteers>();
            foreach (var pending in pendingEvents)
            {
                var getPending = _volunteers.GetAll().Where(m => m.userId == userId && m.eventId == pending.eventId && m.Status == 0).OrderByDescending(m => m.applyVolunteerId).FirstOrDefault();

                if (!pendings.Contains(getPending))
                {
                    pendings.Add(getPending);
                }
            }
            return pendings;
        }
        public List<Volunteers> GetVolunteersEventParticipateByUserId(int userId)
        {
            var acceptedEvents = _volunteers.GetAll().Where(m => m.userId == userId && m.Status == 1).ToList();
            List<Volunteers> participate = new List<Volunteers>();
            foreach (var accept in acceptedEvents)
            {
                var getParticipate = _volunteers.GetAll().Where(m => m.userId == userId && m.eventId == accept.eventId && m.Status == 1).OrderByDescending(m => m.applyVolunteerId).FirstOrDefault();

                if (!participate.Contains(getParticipate))
                {
                    participate.Add(getParticipate);
                }
            }
            return participate;
        }
        public void CheckVolunteerEventEndByUserId(int userId)
        {
            var getEvents = _vw_listOfEvent.GetAll().ToList();
            var endedEvent = getEvents.Where(m => m.End_Date < DateTime.Now).ToList();

            foreach (var evt in endedEvent)
            {
                var getVolEvent = _volunteers.GetAll().Where(m => m.eventId == evt.Event_Id && m.Status == 1).ToList();
                // Move each volunteer record to VolunteersHistory
                foreach (var volunteer in getVolEvent.Where(m => m.userId == userId))
                {
                    var volunteerHistory = new VolunteersHistory
                    {
                        eventId = volunteer.eventId,
                        userId = volunteer.userId,
                        skillId = volunteer.skillId,
                        appliedAt = volunteer.appliedAt,
                        attended = volunteer.attended ?? 0
                    };

                    db.VolunteersHistory.Add(volunteerHistory);
                    db.SaveChanges();
                }
                foreach (var volunteer in getVolEvent)
                {
                    db.sp_RemoveEvent(volunteer.eventId);
                }
            }
        }
    }
}