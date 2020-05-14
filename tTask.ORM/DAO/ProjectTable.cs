using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class ProjectTable
    {
        private AppDbContext db;
        public ProjectTable(AppDbContext db)
        {
            this.db = db;
        }

        public ICollection<UserProject> GetUserProjectsByProjectId(int id)
        {
            var up = db.UserProject.Where(p => p.IdProject == id)
                            .Include(u => u.IdUserNavigation)
                            .ToList();
            return up;
        }

        public void NewProject(Project project, int idUser)
        {

            var up = new UserProject()
            {
                IdProject = project.IdProject,
                IdUser = idUser,
                IdRole = (int)Roles.ProjectLeader,
                Active = "1"
            };
            db.Project.Add(project);
            db.UserProject.Add(up);
            db.SaveChanges();
        }

        public void DeleteProject(int idProject)
        {
            var tasks = db.Task.Where(t => t.IdProject == idProject).ToList();
            foreach (var task in tasks)
            {
                var comments = db.TaskUserComment.Where(c => c.IdTask == task.IdTask).ToList();
                db.TaskUserComment.RemoveRange(comments);

                var ut = db.UserTask.Where(c => c.IdTask == task.IdTask).ToList();
                db.UserTask.RemoveRange(ut);
            }

            db.Task.RemoveRange(tasks);

            var up = db.UserProject.Where(up => up.IdProject == idProject).ToList();
            db.UserProject.RemoveRange(up);

            db.Project.Remove(GetProjectWithIncludesById(idProject));
            db.SaveChanges();
        }

        public void UpdateProject(Project p)
        {
            db.Update<Project>(p);
            db.SaveChanges();
        }

        public Project GetProjectWithIncludesById(int id)
        {
            return db.Project.Where(p => p.IdProject == id).Include(p => p.UserProject).FirstOrDefault();
        }

        public int GetMaxId()
        {
            var max = 0;
            int? id = db.Project.Max(u => (int?)u.IdProject);
            if (id != null)
                max = (int)id;
            return max + 1;
        }

        public Project GetProjectById(int id)
        {
            return db.Project.Find(id);
        }

        public int GetRoleOfUser(int idUser, int idProject)
        {
            var up = db.UserProject.Where(u => u.IdUser == idUser)
                              .Where(u => u.IdProject == idProject)
                              .FirstOrDefault();
            return up.IdRole;
        }

        public UserProject GetUserProjectByIds(int idUser, int idProject)
        {
            var up = db.UserProject.Where(u => u.IdUser == idUser)
                              .Where(u => u.IdProject == idProject)
                              .FirstOrDefault();
            return up;
        }

        public void UpdateUserProject(UserProject up)
        {
            db.Update<UserProject>(up);
            db.SaveChanges();
        }
        public void DeleteUserProject(UserProject up)
        {
            db.Remove<UserProject>(up);
            db.SaveChanges();
        }

        public bool UserCanViewProject(int idUser, int idProject)
        {
            var up = db.UserProject.Where(u => u.IdUser == idUser)
                              .Where(u => u.IdProject == idProject)
                              .FirstOrDefault();
            if (up != null)
            {
                if (up.IdRole == (int)Roles.ProjectLeader || up.IdRole == (int)Roles.ProjectUser)
                {
                    return true;
                }
            }
            return false;
        }

        public bool UserCanControlProject(int idUser, int idProject)
        {
            var up = db.UserProject.Where(u => u.IdUser == idUser)
                              .Where(u => u.IdProject == idProject)
                              .FirstOrDefault();
            if (up != null)
            {
                if (up.IdRole == (int)Roles.ProjectLeader)
                {
                    return true;
                }
            }
            return false;
        }

        public void InsertUserProject(UserProject up)
        {
            db.UserProject.Add(up);
            db.SaveChanges();
        }

        public ICollection<Project> GetProjectsByUserId(int id)
        {
            var projects = new List<Project>();
            var up = db.UserProject.Where(u => u.IdUser == id)
                                    .Where(u => u.IdRole != (int)Roles.ProjectRequest)
                                    .ToList();
            foreach (var u in up)
            {
                projects.Add(db.Project.Find(u.IdProject));
            }

            return projects;
        }

        public ICollection<Project> GetAllProjects()
        {
            return db.Project.Include(p =>p.UserProject).Include(p => p.Task).ToList();
        }

        public int GetNOProject()
        {
            return db.Project.Count<Project>();
        }

        public int GetNOProjectRequest(int idUser)
        {
            return db.UserProject.Where(up => up.IdUser == idUser)
                                .Where(up => up.IdRole == (int)Roles.ProjectRequest)
                                .Count();
        }

        public IEnumerable<UserProject> GetUserProjectRequestsByUserId(int idUser)
        {
            var up = db.UserProject.Where(up => up.IdUser == idUser)
                                    .Where(up => up.IdRole == (int)Roles.ProjectRequest)
                                    .Include(up => up.IdProjectNavigation)
                                    .ToList();
            return up;
        }

    }
}
