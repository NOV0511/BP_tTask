using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class TaskUserCommentTable
    {
        private AppDbContext db;
        public TaskUserCommentTable(AppDbContext db)
        {
            this.db = db;
        }

        public ICollection<TaskUserComment> GetCommentsByTaskId(int id)
        {
            return db.TaskUserComment.Where(t => t.IdTask == id).ToList();
        }

        public void NewComment(TaskUserComment comment)
        {
            db.TaskUserComment.Add(comment);
            db.SaveChanges();
        }

        public int GetMaxId()
        {
            var max = 0;
            int? id = db.TaskUserComment.Max(u => (int?)u.IdComment);
            if (id != null)
                max = (int)id;
            return max + 1;
        }

        public int GetNOCommentsByTaskId(int id)
        {
            return db.TaskUserComment.Where(c => c.IdTask == id).Count();
        }

        public TaskUserComment GetCommentById(int idComment)
        {
            return db.TaskUserComment.Find(idComment);
        }

        public void DeleteComment(TaskUserComment comment)
        {
            db.TaskUserComment.Remove(comment);
            db.SaveChanges();
        }
    }
}
