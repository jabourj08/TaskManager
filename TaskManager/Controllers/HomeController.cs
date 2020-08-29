using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly TaskManagerContext _TaskManager;

        public HomeController(TaskManagerContext taskManager)
        {
            _TaskManager = taskManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult MyTasks()
        {
            //AspNetUsers currentUser = new AspNetUsers();
            //string id = currentUser.Id;

            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (id != null && id != "")
            {
                List<TaskItem> myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).ToList();

                return View(myTasks);
            }
            else
            {
                return View("Areas/Identity/Login");
            }
        }

        // Display Add Task Form
        [HttpGet]
        public IActionResult AddTask(int id)
        {
            return View();
        }

        // Add a task for the user to the db
        [HttpPost]
        public IActionResult AddTask(TaskItem newTaskItem)
        {
            if (ModelState.IsValid)
            {
                string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                newTaskItem.UserId = id;
                newTaskItem.Complete = false;
                _TaskManager.TaskItem.Add(newTaskItem);
                _TaskManager.SaveChanges(); // Save Task to db
            }

            return RedirectToAction("MyTasks");
        }

        // Update task for a user
        public IActionResult UpdateTask(int id)
        {
            TaskItem currentTask = _TaskManager.TaskItem.Find(id);

            if (currentTask == null)
            {
                return RedirectToAction("MyTasks");
            }
            else
            {
                return View(currentTask);
            }
        }

        // Save the updates to the database
        public IActionResult SaveUpdates(TaskItem updatedTask)
        {
            TaskItem originalTask = _TaskManager.TaskItem.Find(updatedTask.Id);

            originalTask.TaskName = updatedTask.TaskName;
            originalTask.TaskDetails = updatedTask.TaskDetails;
            originalTask.TaskPriority = updatedTask.TaskPriority;
            originalTask.DueDate = updatedTask.DueDate;
            originalTask.Complete = updatedTask.Complete;

            _TaskManager.Entry(originalTask).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _TaskManager.Update(originalTask);
            _TaskManager.SaveChanges();

            return RedirectToAction("MyTasks");
        }

        // Change Completion status ONLY
        public IActionResult ChangeTaskStatus(int id)
        {
            TaskItem originalTask = _TaskManager.TaskItem.Find(id);

            if (originalTask.Complete == false)
            {
                originalTask.Complete = true;
            }
            else
            {
                originalTask.Complete = false;
            }

            _TaskManager.Entry(originalTask).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _TaskManager.Update(originalTask);
            _TaskManager.SaveChanges();

            return RedirectToAction("MyTasks");
        }

        // DESTROY task for a user
        public IActionResult DeleteTask(int id)
        {
            TaskItem currentTask = _TaskManager.TaskItem.Find(id);

            if(currentTask != null)
            {
                _TaskManager.TaskItem.Remove(currentTask);
                _TaskManager.SaveChanges();
            }

            return RedirectToAction("MyTasks");
        }

        public IActionResult SearchTasks(string search)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (id != null && id != "" && search != null && search != "")
            {
                List<TaskItem> searchedTasks = _TaskManager.TaskItem.Where(x => x.TaskDetails.ToLower().Contains(search.ToLower()) || x.TaskName.ToLower().Contains(search.ToLower())).ToList();

                return View("MyTasks", searchedTasks);
            }
            else
            {
                List<TaskItem> myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).ToList();

                return View("MyTasks", myTasks);
            }
        }

        public IActionResult SortTasks(string sort)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (id != null && id != "" && sort != null && sort != "")
            {
                //List<TaskItem> myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).ToList();
                List<TaskItem> myTasks = new List<TaskItem>();

                if (sort == "TaskName")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderBy(x => x.TaskName).ToList();
                }
                else if (sort == "TaskNameDesc")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderByDescending(x => x.TaskName).ToList();
                }
                else if (sort == "LowPriority")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderByDescending(x => x.TaskPriority.Contains("Low")).ToList();
                }
                else if (sort == "MediumPriority")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderByDescending(x => x.TaskPriority.Contains("Medium")).ToList();
                }
                else if (sort == "HighPriority")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderByDescending(x => x.TaskPriority.Contains("High")).ToList();
                }
                else if (sort == "DueDate")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderBy(x => x.DueDate).ToList();
                }
                else if (sort == "DueDateDesc")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderByDescending(x => x.DueDate).ToList();
                }
                else if (sort == "Complete")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderBy(x => x.Complete.Value == false).ToList();
                }
                else if (sort == "Incomplete")
                {
                    myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).OrderBy(x => x.Complete.Value == true).ToList();
                }

                return View("MyTasks", myTasks);
            }
            else
            {
                List<TaskItem> myTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).ToList();

                return View("MyTasks", myTasks);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
