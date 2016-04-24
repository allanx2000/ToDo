using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ToDo.Client.Core.Tasks
{
    [Serializable]
    public class TaskLog
    {
        [Required]
        public int TaskID { get; set; }
        [XmlIgnore]
        public virtual TaskItem Task { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool Completed { get; set; }
    }
}
