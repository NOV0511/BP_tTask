using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using tTask.Models.Forms;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.NormalUser))]
    [Authorize(Policy = "TenantPolicy")]
    public class TasksController : Controller
    {
        private readonly UserTable _userTable;
        private readonly UserTaskTable _userTaskTable;
        private readonly TaskTable _taskTable;
        private readonly ProjectTable _projectTable;
        private readonly TaskUserCommentTable _taskUserCommentTable;
        private readonly TenantTable _tenantTable;
        private readonly ServiceOrderTable _serviceOrderTable;
        private readonly NotificationTable _notificationTable;


        public TasksController(UserTable userTable, UserTaskTable userTaskTable, TaskTable taskTable, TaskUserCommentTable taskUserCommentTable,
            ProjectTable projectTable, TenantTable tenantTable, ServiceOrderTable serviceOrderTable, NotificationTable notificationTable)
        {
            _userTable = userTable;
            _userTaskTable = userTaskTable;
            _taskTable = taskTable;
            _taskUserCommentTable = taskUserCommentTable;
            _projectTable = projectTable;
            _tenantTable = tenantTable;
            _serviceOrderTable = serviceOrderTable;
            _notificationTable = notificationTable;
        }
        public IActionResult Index( string search = null)
        {

            var idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            var settings = _userTable.GetSettingsbyUserId(idUser);
            var idService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string));

            IOrderedEnumerable<Task> allTasks;
            IOrderedEnumerable<Task> completed;
            if (idService != (int)Services.Basic && settings.CustomizeView == "1")
            {
                allTasks = _taskTable.GetUserTasksActive(_userTable.GetUserId(HttpContext.User.Identity.Name)).OrderBy(task => task.Deadline);
                completed = _taskTable.GetUserTasksCompletedActive(_userTable.GetUserId(HttpContext.User.Identity.Name)).OrderBy(task => task.Deadline);
            }
            else
            {
                allTasks = _taskTable.GetUserTasks(_userTable.GetUserId(HttpContext.User.Identity.Name)).OrderBy(task => task.Deadline);
                completed = _taskTable.GetUserTasksCompleted(_userTable.GetUserId(HttpContext.User.Identity.Name)).OrderBy(task => task.Deadline);
            }

            var model = new TasksViewModel
            {
                UpcommingTasks =  allTasks.Where(t => t.Deadline >= DateTime.Now),
                ExpiredTasks = allTasks.Where(t => t.Deadline < DateTime.Now),
                CompletedTasks = completed,
                UsersProjects = new List<SelectListItem>(),
                Priority = new List<SelectListItem>(),
                IdService = idService,
                SettingsColoring = settings.Coloring == "1",
                TaskForm = new TaskForm()
            };
            if(search != null)
            {
                model.UpcommingTasks = allTasks.Where(t => t.Deadline >= DateTime.Now).Where(t => t.Name.ToLower().Contains(search.ToLower()));
                model.ExpiredTasks = allTasks.Where(t => t.Deadline < DateTime.Now).Where(t => t.Name.ToLower().Contains(search.ToLower()));
                model.CompletedTasks = completed.Where(t => t.Name.ToLower().Contains(search.ToLower()));
            }

            foreach (var project in _projectTable.GetProjectsByUserId(idUser)) 
            {
                model.UsersProjects.Add(new SelectListItem { Text = project.Name, Value = project.IdProject.ToString() });
            }
            foreach (string name in Enum.GetNames(typeof(Priority)))
            {
                model.Priority.Add(new SelectListItem { Text = name, Value = name });
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult GetTask(int id)
        {
            return Json(_taskTable.GetTaskById(id)); 
        }

        [HttpGet]
        public IActionResult GetUser(int id)
        {
            return Json(_userTable.GetUserPropertiesById(id));
        }

        [HttpGet]
        public IActionResult GetComments(int id)
        {
            return Json(_taskUserCommentTable.GetCommentsByTaskId(id).OrderBy(c => c.Created));
        }

        [HttpPost]
        public void NewComment(string text, int idTask)
        {
            var idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            var comment = new TaskUserComment()
            {
                IdComment = _taskUserCommentTable.GetMaxId(),
                IdTask = idTask,
                Text = text,
                Created = DateTime.Now,
                IdUser = idUser
            };
            _taskUserCommentTable.NewComment(comment);

            var task = _taskTable.GetTaskById(idTask);
            var msg = "Your task " + task.Name + " has new comment";
            _notificationTable.NotifyUser(task.IdUser, msg);
        }

        [HttpGet]
        public IActionResult GetLoggedUserId()
        {
            return Json(_userTable.GetUserId(HttpContext.User.Identity.Name));
        }


        [HttpPost]
        public void CompleteTask(int idTask)
        {
            var user = _userTable.GetUserById(_userTable.GetUserId(HttpContext.User.Identity.Name));
            var ut = new UserTask()
            {
                IdTask = idTask,
                Completed = DateTime.Now,
                IdUser = user.Id
            };
            _userTaskTable.CompleteTask(ut);

            var task = _taskTable.GetTaskById(idTask);
            var msg = "User " + user.FirstName + " " + user.Surname + " has completed your task " + task.Name + ".";
            _notificationTable.NotifyUser(user.Id, msg);
        }

        [HttpPost]
        public IActionResult Search(string taskName)
        {
            return RedirectToAction("Index", new { search = taskName });
        }



        [HttpPost]
        public IActionResult AddNewTask(TaskForm taskForm)
        {
            var idTask = _taskTable.GetMaxId();
            var task = new Task()
            {
                IdTask = idTask,
                Name = taskForm.Name,
                Description = taskForm.Description,
                Priority = taskForm.Priority,
                Created = DateTime.Now,
                Deadline = taskForm.Deadline,
                IdUser = _userTable.GetUserId(HttpContext.User.Identity.Name),
                IdProject = (int)taskForm.IdProject
            };
            _taskTable.InsertTask(task);
            foreach (var id in taskForm.AssignedTo)
            {
                var ut = new UserTask()
                {
                    IdTask = idTask,
                    IdUser = id,
                };
                _userTaskTable.InsertUserTask(ut);
                var msg = "Task " + taskForm.Name + " has been assigned to you.";
                _notificationTable.NotifyUser(id, msg);
            }
            if (taskForm.From == "project")
                return RedirectToAction("Index", "Project", new { taskForm.IdProject });

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetProjectUsers(int idProject)
        {
            var users = _userTable.GetUsersInProjectByProjectId(idProject);
            var json = JsonSerializer.Serialize(users);
            return Json(users);
        }

        [HttpGet]
        public IActionResult CanComment(int idTask)
        {
            var idService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string));
            var noComments = _taskUserCommentTable.GetNOCommentsByTaskId(idTask);

            if ((idService == (int)Services.Basic && noComments >= 5) || (idService == (int)Services.Pro && noComments >= 10))
                return Json(false);

            return Json(true);
        }


        [HttpPost]
        public IActionResult DeleteTask(int idTask, int idProject = 0)
        {
            _taskTable.DeleteTask(idTask);
            if(idProject == 0)
                return RedirectToAction("Index");

            return RedirectToAction("Index", "Project", new { idProject });
        }



        [HttpPost]
        public IActionResult UpdateTask(TaskForm taskForm)
        {
            var task = _taskTable.GetTaskById((int)taskForm.IdTask);
            task.Name = taskForm.Name;
            task.Description = taskForm.Description;
            task.Deadline = taskForm.Deadline;
            task.Priority = taskForm.Priority;

            _taskTable.UpdateTask(task);

            var userTasks = _taskTable.GetUserTasksByTaskId((int)taskForm.IdTask);
            var ids = new List<int>();
            foreach (var ut in userTasks)
            {
                ids.Add(ut.IdUser);
            }
            foreach (var id in taskForm.AssignedTo)
            {
                if (ids.Contains(id))
                {
                    ids.Remove(id);
                }
                else
                {
                    var ut = new UserTask()
                    {
                        IdTask = (int)taskForm.IdTask,
                        IdUser = id,
                    };
                    _userTaskTable.InsertUserTask(ut);
                    var msg = "Task " + taskForm.Name + " has been newly assigned to you.";
                    _notificationTable.NotifyUser(id, msg);
                }
            }
            foreach (var id in ids)
            {
                _taskTable.DeleteUserTask(id, (int)taskForm.IdTask);
                var msg = "You have been renoved from task " + taskForm.Name + ".";
                _notificationTable.NotifyUser(id, msg);
            }
            if (taskForm.From == "project")
                return RedirectToAction("Index", "Project", new { task.IdProject });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public void DeleteComment(int idComment)
        {
            var comment = _taskUserCommentTable.GetCommentById(idComment);
            _taskUserCommentTable.DeleteComment(comment);
        }

    }
}