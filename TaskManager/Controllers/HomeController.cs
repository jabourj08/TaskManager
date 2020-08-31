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
            //get id of logged in user
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
            //Make sure all fields match db fields
            if (ModelState.IsValid)
            {
                string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                //assign foreign key by logged in user
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

        // Search tasks by input string.
        public IActionResult SearchTasks(string search, string sort)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            TempData["Sort"] = sort;
            TempData["Search"] = search;

            if (id != null && id != "" && search != null && search != "")
            {
                List<TaskItem> searchedTasks = _TaskManager.TaskItem.Where(x => x.TaskDetails.ToLower().Contains(search.ToLower()) && x.UserId == id || x.UserId == id && x.TaskName.ToLower().Contains(search.ToLower())).ToList();

                searchedTasks = SortTasks(searchedTasks);

                return View("MyTasks", searchedTasks);
            }
            else
            {
                List<TaskItem> searchedTasks = _TaskManager.TaskItem.Where(x => x.UserId == id).ToList();

                //send filtered tasks to sort
                searchedTasks = SortTasks(searchedTasks);

                return View("MyTasks", searchedTasks);
            }
        }

        //Sort tasks by criteria
        public List<TaskItem> SortTasks(List<TaskItem> searchedTasks)
        {
            string sort = TempData["Sort"].ToString();

            if (sort == "TaskName")
            {
                searchedTasks = searchedTasks.OrderBy(x => x.TaskName).ToList();
            }
            else if (sort == "TaskNameDesc")
            {
                searchedTasks = searchedTasks.OrderByDescending(x => x.TaskName).ToList();
            }
            else if (sort == "LowPriority")
            {
                searchedTasks = searchedTasks.OrderByDescending(x => x.TaskPriority.Contains("Low")).ThenBy(x => x.DueDate).ToList();
            }
            else if (sort == "MediumPriority")
            {
                searchedTasks = searchedTasks.OrderByDescending(x => x.TaskPriority.Contains("Medium")).ThenBy(x => x.DueDate).ToList();
            }
            else if (sort == "HighPriority")
            {
                searchedTasks = searchedTasks.OrderByDescending(x => x.TaskPriority.Contains("High")).ThenBy(x => x.DueDate).ToList();
            }
            else if (sort == "DueDate")
            {
                searchedTasks = searchedTasks.OrderBy(x => x.DueDate).ToList();
            }
            else if (sort == "DueDateDesc")
            {
                searchedTasks = searchedTasks.OrderByDescending(x => x.DueDate).ToList();
            }
            else if (sort == "Complete")
            {
                searchedTasks = searchedTasks.OrderBy(x => x.Complete.Value == false).ThenBy(x => x.DueDate).ToList();
            }
            else if (sort == "Incomplete")
            {
                searchedTasks = searchedTasks.OrderBy(x => x.Complete.Value == true).ThenBy(x => x.DueDate).ToList();
            }

            return searchedTasks;
        }

        public IActionResult ShowFullTask(int id)
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
