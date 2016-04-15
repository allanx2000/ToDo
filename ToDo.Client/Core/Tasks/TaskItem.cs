using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core.Lists;

namespace ToDo.Client.Core.Tasks
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

        public DateTime NextReminder { get; set; }
        public virtual TaskFrequency Frequency { get; set; }

        //Navigation/References
        public virtual List<Comment> Comments { get; set; }

        public virtual TaskItem Parent { get; set; }

        public virtual List<TaskItem> Children { get; set; }
        public virtual TaskList List { get; set; };



    }
}
