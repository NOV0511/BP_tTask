using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class UserTable
    {
        private AppDbContext db;
        public UserTable(AppDbContext db)
        {
            this.db = db;
        }
        public int GetMaxId()
        {
            var max = 0;
            int? id = db.User.Max(u => (int?)u.Id);
            if (id != null)
                max = (int)id;
            return max + 1;
        }

        public User GetUserWithProjects(string userName)
        {
            var user = db.User.Where(u => u.UserName == userName)
                            .Include(u => u.UserProject)
                                .ThenInclude(p => p.IdProjectNavigation).FirstOrDefault();
            
            return user;
        }

        public int GetUserId(string userName)
        {
            var user = db.User.Where(u => u.UserName == userName).FirstOrDefault();

            return user.Id;
        }

        public User GetUserById(int id)
        {
            var user = db.User.Find(id);

            return user;
        }

        public User GetUserPropertiesById(int id)
        {
            var user = db.User.Where(u => u.Id == id)
                            .Select(u => new User(){ 
                                FirstName =u.FirstName, 
                                Surname = u.Surname, 
                                Photopath =u.Photopath,
                                Email = u.Email,
                                PhoneNumber = u.PhoneNumber
                            })
                            .FirstOrDefault();

            return user;
        }

        public ICollection<User> GetUsersOutOfProjectByProjectId(int id)
        {
            var users = db.User.Include(u => u.UserProject)
                                    .Where(p => !p.UserProject.Any(up => up.IdProject == id))
                                    .ToList();
            return users;
        }

        public ICollection<User> GetUsersInProjectByProjectId(int id)
        {
            var up = db.UserProject
                            .Where(p => p.IdProject == id)
                            .Where(u => u.IdRole != (int)Roles.ProjectRequest)
                            .ToList();
            var users = new List<User>();

            foreach (var u in up)
            {
                var user = GetUserById(u.IdUser);
                user.UserProject = null;
                users.Add(user);
            }
            return users;
        }

        public void UpdateUser(User u)
        {
            db.Update<User>(u);
            db.SaveChanges();
        }

        public ICollection<User> GetAllUsers()
        {
            var users = db.User.Select(u => new User()
                                        {
                                            Id = u.Id,
                                            FirstName = u.FirstName,
                                            Surname = u.Surname,
                                            Photopath = u.Photopath,
                                            Email = u.Email,
                                            PhoneNumber = u.PhoneNumber
                                        })
                                .ToList();
            return users;
        }
        public int GetNOUsers()
        {
            return db.User.Count<User>();
        }

        public User GetUserWithSettingsById(int id)
        {
            var user = db.User.Where(u => u.Id == id)
                            .Include(u => u.UserSettings)
                            .FirstOrDefault();

            return user;
        }

        public void UpdateSettings(UserSettings settings)
        {
            db.Update<UserSettings>(settings);
            db.SaveChanges();
        }

        public void InsertSettings(UserSettings settings)
        {
            db.UserSettings.Add(settings);
            db.SaveChanges();
        }
        public UserSettings GetSettingsbyUserId(int id)
        {
            return db.UserSettings.Find(id);
        }



    }
}
