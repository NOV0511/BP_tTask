using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class TaskTable
    {
        private AppDbContext db;
        private readonly UserTable _ut;
        private readonly ProjectTable _projectTable;
        public TaskTable(AppDbContext db, UserTable ut, ProjectTable projectTable)
        {
            this.db = db;
            _ut = ut;
            _projectTable = projectTable;
        }

        public ICollection<Task> GetUserTasks(int id)
        {

            var tasks = db.Task.Where(t => t.IdUser == id)
                .Where(t => t.Completed == null)
                .Include(t => t.UserTask)
                .ToList();

            tasks.AddRange(db.Task.Where(t => t.IdUser != id)
                .Include(t => t.UserTask)
                .Where(ut => ut.UserTask.Any(user => user.IdUser == id && user.Completed == null))
                .ToList());

            foreach (var task in tasks)
            {
                task.IdUserNavigation = _ut.GetUserPropertiesById(task.IdUser);
                task.IdProjectNavigation = _projectTable.GetProjectById(task.IdProject);
                foreach(var ut in task.UserTask)
                {
                    ut.IdUserNavigation = _ut.GetUserPropertiesById(ut.IdUser);
                }
            }

            return tasks;
        }

        public ICollection<Task> GetUserTasksCompleted(int id)
        {

            var tasks = db.Task.Where(t => t.IdUser == id)
                .Where(t => t.Completed != null)
                .Include(t => t.UserTask)
                .ToList();

            tasks.AddRange(db.Task.Where(t => t.IdUser != id)
                .Include(t => t.UserTask)
                .Where(ut => ut.UserTask.Any(user => user.IdUser == id && user.Completed != null))
                .ToList());

            foreach (var task in tasks)
            {
                task.IdUserNavigation = _ut.GetUserPropertiesById(task.IdUser);
                task.IdProjectNavigation = _projectTable.GetProjectById(task.IdProject);
                foreach (var ut in task.UserTask)
                {
                    ut.IdUserNavigation = _ut.GetUserPropertiesById(ut.IdUser);
                }
            }

            return tasks;
        }

        public ICollection<Task> GetUserTasksCloseDeadline(int id)
        {
            var tasks = db.Task.Where(t => t.Completed == null)
                .Where(t => t.Deadline > DateTime.Now)
                .Where(t => t.Deadline < DateTime.Now.AddHours(1))
                .Include(t => t.UserTask)
                .Where(ut => ut.UserTask.Any(user => user.IdUser == id))
                .ToList();

            return tasks;
        }

        public ICollection<Task> GetUserTasksActive(int id)
        {
            var idProjectsListuserProjects = db.UserProject.Where(u => u.IdUser == id)
                                            .Where(u => u.Active == "1")
                                            .Select(p => p.IdProject)
                                            .ToList();

            var tasks = db.Task.Where(t => t.IdUser == id)
                .Where(t => t.Completed == null)
                .Where(t => idProjectsListuserProjects.Contains(t.IdProject))
                .Include(t => t.UserTask)
                .ToList();

            tasks.AddRange(db.Task.Where(t => t.IdUser != id)
                .Where(t => idProjectsListuserProjects.Contains(t.IdProject))
                .Include(t => t.UserTask)
                .Where(ut => ut.UserTask.Any(user => user.IdUser == id && user.Completed == null))
                .ToList());

            foreach (var task in tasks)
            {
                task.IdUserNavigation = _ut.GetUserPropertiesById(task.IdUser);
                task.IdProjectNavigation = _projectTable.GetProjectById(task.IdProject);
                foreach (var ut in task.UserTask)
                {
                    ut.IdUserNavigation = _ut.GetUserPropertiesById(ut.IdUser);
                }
            }

            return tasks;
        }

        public ICollection<Task> GetUserTasksCompletedActive(int id)
        {
            var idProjectsListuserProjects = db.UserProject.Where(u => u.IdUser == id)
                                            .Where(u => u.Active == "1")
                                            .Select(p => p.IdProject)
                                            .ToList();

            var tasks = db.Task.Where(t => t.IdUser == id)
                .Where(t => t.Completed != null)
                .Where(t => idProjectsListuserProjects.Contains(t.IdProject))
                .Include(t => t.UserTask)
                .ToList();

            tasks.AddRange(db.Task.Where(t => t.IdUser != id)
                .Where(t => idProjectsListuserProjects.Contains(t.IdProject))
                .Include(t => t.UserTask)
                .Where(ut => ut.UserTask.Any(user => user.IdUser == id && user.Completed != null))
                .ToList());

            foreach (var task in tasks)
            {
                task.IdUserNavigation = _ut.GetUserPropertiesById(task.IdUser);
                task.IdProjectNavigation = _projectTable.GetProjectById(task.IdProject);
                foreach (var ut in task.UserTask)
                {
                    ut.IdUserNavigation = _ut.GetUserPropertiesById(ut.IdUser);
                }
            }

            return tasks;
        }

        public ICollection<Task> GetUserTasksFilter(int id, string name)
        {

            var tasks = db.Task
                .Where(t => t.IdUser == id)
                .Where(t => t.Completed == null)
                .Where(t => t.Name.Contains(name))
                .Include(t => t.UserTask)
                .ToList();

            tasks.AddRange(db.Task
                .Where(t => t.Completed == null)
                .Where(t => t.Name.Contains(name))
                .Where(t => t.IdUser != id)
                .Include(t => t.UserTask)
                .Where(ut => ut.UserTask.Any(user => user.IdUser == id))
                .ToList());

            foreach (var task in tasks)
            {
                task.IdUserNavigation = _ut.GetUserPropertiesById(task.IdUser);
                task.IdProjectNavigation = _projectTable.GetProjectById(task.IdProject);
                foreach (var ut in task.UserTask)
                {
                    ut.IdUserNavigation = _ut.GetUserPropertiesById(ut.IdUser);
                }
            }

            return tasks;
        }

        public Task GetTaskById(int id)
        {
            var task = db.Task.Where(t => t.IdTask == id)
                            .Include(t => t.UserTask)
                            .FirstOrDefault();

            return task;
        }

        public void CompleteTask(int id)
        {
            var task = GetTaskById(id);
            task.Completed = DateTime.Now;
            db.Update<Task>(task);
            db.SaveChanges();
        }

        public ICollection<Task> GetTasksByProjectId(int id)
        {

            var tasks = db.Task.Where(t => t.IdProject == id)
                .Include(t => t.UserTask)
                .ToList();


            foreach (var task in tasks)
            {
                task.IdUserNavigation = _ut.GetUserPropertiesById(task.IdUser);
                foreach (var ut in task.UserTask)
                {
                    ut.IdUserNavigation = _ut.GetUserPropertiesById(ut.IdUser);
                }
            }

            return tasks;
        }
        public int GetMaxId()
        {
            var max = 0;
            int? id = db.Task.Max(u => (int?)u.IdTask);
            if (id != null)
                max = (int)id;
            return max + 1;
        }

        public void InsertTask(Task t)
        {
            db.Task.Add(t);
            db.SaveChanges();
        }


        public void DeleteTask(int idTask)
        {
            var task = GetTaskById(idTask);

            var comments = db.TaskUserComment.Where(c => c.IdTask == task.IdTask).ToList();
            db.TaskUserComment.RemoveRange(comments);

            var ut = db.UserTask.Where(c => c.IdTask == task.IdTask).ToList();
            db.UserTask.RemoveRange(ut);

            db.Task.Remove(task);

            db.SaveChanges();
        }

        public List<UserTask> GetUserTasksByTaskId(int idTask)
        {
            return db.UserTask.Where(ut => ut.IdTask == idTask).ToList();
        }

        public void DeleteUserTask(int idUser, int idTask)
        {
            var ut = db.UserTask.Where(ut => ut.IdUser == idUser)
                                .Where(ut => ut.IdTask == idTask)
                                .FirstOrDefault();
            db.UserTask.Remove(ut);
            db.SaveChanges();
        }

        public void UpdateTask(Task task)
        {
            db.Update<Task>(task);
            db.SaveChanges();
        }
    }
}
