using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class NotificationTable
    {
        private readonly AppDbContext _db;
        private readonly UserTable _userTable;
        private readonly TaskTable _taskTable;
        private readonly ServiceOrderTable _serviceOrderTable;

        public NotificationTable(AppDbContext db, UserTable userTable, TaskTable taskTable, ServiceOrderTable serviceOrderTable)
        {
            _db = db;
            _userTable = userTable;
            _taskTable = taskTable;
            _serviceOrderTable = serviceOrderTable;
        }

        public int GetMaxId() {
            var max = 0;
            int? id = _db.UserNotification.Max(u => (int?)u.IdNotification);
            if (id != null)
                max = (int)id;
            return max + 1;
        }

        public UserNotification GetNotificationById(int id)
        {
            return _db.UserNotification.Find(id);
        }

        public void NotifyUser(int idUser, string text)
        {
            var notification = new UserNotification()
            {
                IdNotification = GetMaxId(),
                Text = text,
                Created = DateTime.Now,
                Read = "0",
                IdUser = idUser
            };
            _db.UserNotification.Add(notification);
            _db.SaveChanges();
        }


        public void ReadNotification(int id)
        {
            var notification = GetNotificationById(id);
            notification.Read = "1";
            _db.Update<UserNotification>(notification);
            _db.SaveChanges();
        }

        public void ReadAllNotifications(int idUser)
        {
            var notifications = _db.UserNotification.Where(u => u.IdUser == idUser).Where(n => n.Read == "0").ToList();
            notifications.ForEach(n => n.Read = "1");
            _db.SaveChanges();
        }

        public ICollection<UserNotification> GetUnreadNotifiactionsByUserId(int idUser)
        {
            var notifications = _db.UserNotification.Where(u => u.IdUser == idUser).Where(n => n.Read == "0").ToList();
            foreach (var n in notifications)
            {
                n.IdUserNavigation = null;
            }
            return notifications;
        }

        public ICollection<UserNotification> GetReadNotifiactionsByUserId(int idUser)
        {
            var notifications = _db.UserNotification.Where(u => u.IdUser == idUser).Where(n => n.Read == "1").ToList();
            foreach (var n in notifications)
            {
                n.IdUserNavigation = null;
            }
            return notifications;
        }

        public int GetCountUnreadByUserId(int idUser)
        {
            return _db.UserNotification.Where(u => u.IdUser == idUser).Where(n => n.Read == "0").Count();
        }

        public void NotifyCloseDeadlineTasks(int idUser)
        {
            var tasks = _taskTable.GetUserTasksCloseDeadline(idUser);
            if(tasks != null)
            {
                foreach (var task in tasks)
                {
                    var msg = "Deadline on task " + task.Name + " is in less than one hour!";
                    NotifyUser(task.IdUser, msg);
                    foreach (var ut in task.UserTask)
                    {
                        if (ut.IdUser != task.IdUser)
                        {
                            msg = "Deadline on task " + task.Name + " is in less than one hour!";
                            NotifyUser(ut.IdUser, msg);
                        }
                    }
                }
            }
        }

        public void NotifyClosePaymentExpirationDate(int idTenant)
        {
            var noDays = _serviceOrderTable.GetDaysToExpireByTenantId(idTenant);
            if(noDays < 4)
            {
                var msg = "Do not forget to pay for your subscription! Subscription will expire in " + noDays + " days.";
                NotifyUser(1, msg);
            }
        }
    }
}
