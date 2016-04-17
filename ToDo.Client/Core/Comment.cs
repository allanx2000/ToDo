using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client.Core
{
    public class Comment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Text { get; set; }

        public int OwnerId { get; set; }
        public virtual TaskItem Owner { get; set; }
    }
}