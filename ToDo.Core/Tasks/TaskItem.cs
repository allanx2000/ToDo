using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Core.Tasks
{
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Completed { get; set; }
        public DateTime DueDate { get; set; }

        public List<Comment> Comments { get; set; }
        public DateTime NextReminder { get; set; }

        //Navigation/References
        public List<TaskItem> Children;
        public int ListId;

    }
}
