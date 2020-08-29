using System;
using System.Collections.Generic;

namespace TaskManager.Models
{
    public partial class TaskItem
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string TaskName { get; set; }
        public string TaskDetails { get; set; }
        public string TaskPriority { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? Complete { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
