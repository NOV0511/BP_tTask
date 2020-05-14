using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class User : IdentityUser<int>
    {
        public User()
        {
            Task = new HashSet<Task>();
            TaskUserComment = new HashSet<TaskUserComment>();
            UserProject = new HashSet<UserProject>();
            UserTask = new HashSet<UserTask>();
            UserNotification = new HashSet<UserNotification>();
        }

        public override int Id { get; set; }
        public override string Email { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public override string PhoneNumber { get; set; }
        public string Photopath { get; set; }
        public int IdTenant { get; set; }

        public virtual Tenant IdTenantNavigation { get; set; }
        public virtual UserSettings UserSettings { get; set; }
        public virtual ICollection<Task> Task { get; set; }
        public virtual ICollection<TaskUserComment> TaskUserComment { get; set; }
        public virtual ICollection<UserProject> UserProject { get; set; }
        public virtual ICollection<UserTask> UserTask { get; set; }
        public virtual ICollection<UserNotification> UserNotification { get; set; }
    }
}
