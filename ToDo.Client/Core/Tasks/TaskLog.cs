using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Client.Core.Tasks
{
    public class TaskLog
    {
        [Required]
        public int TaskID { get; set; }
        public virtual TaskItem Task { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool Completed { get; set; }
    }
}
