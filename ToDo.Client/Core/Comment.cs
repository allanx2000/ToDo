﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client.Core
{
    [Serializable]
    public class Comment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentID { get; set; }
        public DateTime Created { get; set; }
        public string Text { get; set; }

        public int OwnerId { get; set; }

        [XmlIgnore]
        public virtual TaskItem Owner { get; set; }
    }
}