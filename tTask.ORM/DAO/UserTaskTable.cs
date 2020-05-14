using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class UserTaskTable
    {
        private AppDbContext db;
        private readonly TaskTable _tt;
        public UserTaskTable(AppDbContext db, TaskTable tt)
        {
            this.db = db;
            _tt = tt;
        }

        public void CompleteTask(UserTask ut)
        {
            db.Update<UserTask>(ut);
            db.SaveChanges();

            if (AllCompleted(ut.IdTask))
            {
                _tt.CompleteTask(ut.IdTask);
            }
        }

        public bool AllCompleted(int id)
        {
            var ut = db.UserTask.Where(t => t.IdTask == id).ToList();
            bool allDone = true;
            foreach (var t in ut)
            {
                if(t.Completed == null)
                {
                    allDone = false;
                    break;
                }
            }
            return allDone;
        }

        public void InsertUserTask(UserTask ut)
        {
            db.UserTask.Add(ut);
            db.SaveChanges();
        }
    }
}
