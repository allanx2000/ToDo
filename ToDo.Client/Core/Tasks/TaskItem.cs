using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ToDo.Client.Core.Lists;

namespace ToDo.Client.Core.Tasks
{
    [Serializable]
    public class TaskItem
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskItemID { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        public int Priority { get; set; }

        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Updated { get; set; }
        public DateTime? Completed { get; set; }
        public DateTime? DueDate { get; set; }

        //TODO: Add a StartDate Field
        public DateTime? NextReminder { get; set; }

        public int? FrequencyId { get; set; }

        public TaskFrequency? Frequency { get; set; }

        //Navigation/References
        public virtual List<Comment> Comments { get; set; }
        
        public int? ParentID { get; set; }

        [XmlIgnore]
        public virtual TaskItem Parent { get; set; }
        [XmlIgnore]
        public List<TaskItem> Children { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public int ListID { get; set; }
        [XmlIgnore]
        public virtual TaskList List { get; set; }

        public DateTime? StartDate { get; set; }
    }
}
