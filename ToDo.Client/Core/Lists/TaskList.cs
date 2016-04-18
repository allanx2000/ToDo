using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client.Core.Lists
{
    public class TaskList
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskListID { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }

        [Required]
        public ListType Type { get; set; }

        public ICollection<TaskItem> TaskItems
        {
            get; set;
        }
        
    }
}
