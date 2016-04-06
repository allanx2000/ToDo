using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Core.Tasks;

namespace ToDo.Core.Lists
{
    class BaseList
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }

        public List<TaskItem> Tasks;

    }
}
