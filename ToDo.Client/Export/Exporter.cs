using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ToDo.Client.Core;
using ToDo.Client.Core.Lists;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client.Export
{
    public static class Exporter
    {
        [Serializable]
        public class Bundle
        {
            public List<Comment> Comments { get; set; }
            public List<TaskList> Lists { get; set; }
            public List<TaskLog> Log { get; set; }
            public List<TaskItem> Tasks { get; set; }
        }

        public static void Export(string path)
        {
            Bundle bundle = new Bundle()
            {
                Lists = Workspace.Instance.Lists.ToList(),
                Log = Workspace.Instance.TasksLog.ToList(),
                Tasks = Workspace.Instance.Tasks.ToList(),
                Comments = Workspace.Instance.Comments.ToList()
            };

            XmlSerializer xser = GetSerializer();
            StreamWriter sw = new StreamWriter(path);
            xser.Serialize(sw, bundle);
            sw.Close();
            
        }

        private static XmlSerializer GetSerializer()
        {
            return new XmlSerializer(typeof(Bundle));
        }

        public static void Import(string importPath) //, out List<string> errors
        {
            //errors = new List<string>();

            //TODO: out List of errors
            //TODO: verify duplicate names not allowed?

            var xser = GetSerializer();
            Bundle bundle;

            using (StreamReader sr = new StreamReader(importPath))
            {
                bundle = (Bundle) xser.Deserialize(sr);
                sr.Close();
            }

            //Old ID, new Id
            Dictionary<int,int> lists = new Dictionary<int, int>();

            var DB = Workspace.Instance;

            //Lists
            foreach (var l in bundle.Lists)
            {
                var exists = DB.Lists.FirstOrDefault(x => x.Title == l.Title);

                if (exists != null)
                    lists.Add(l.TaskListID, exists.TaskListID);
                else
                {
                    int oldId = l.TaskListID;

                    l.TaskListID = 0;
                    DB.Lists.Add(l);
                    DB.SaveChanges();

                    lists.Add(oldId, l.TaskListID);
                }
            }

            //Tasks
            Dictionary<int, int> tasks = new Dictionary<int, int>();
            
            List<TaskItem> sortedTasks = new List<TaskItem>();
            var tmp = from t in bundle.Tasks
                      where t.ParentID == null
                      select t;
            sortedTasks.AddRange(tmp);

            tmp = from t in bundle.Tasks
                  where t.ParentID != null
                  orderby t.ParentID ascending
                  select t;
            sortedTasks.AddRange(tmp);

            foreach (var t in sortedTasks)
            {
                t.ListID = lists[t.ListID];

                var exists = DB.Tasks.FirstOrDefault(x => x.Title == t.Title && x.ListID == x.ListID);

                if (exists != null)
                    tasks.Add(t.TaskItemID, exists.TaskItemID);
                else
                {
                    int oldId = t.TaskItemID;
                    t.TaskItemID = 0;

                    if (t.ParentID != null)
                        t.ParentID = tasks[t.ParentID.Value];

                    DB.Tasks.Add(t);
                    DB.SaveChanges();

                    tasks.Add(oldId, t.TaskItemID);
                }
            }

            //Logs
            foreach (var l in bundle.Log)
            {
                l.TaskID = tasks[l.TaskID];

                var existing = DB.TasksLog.FirstOrDefault(
                    x => x.TaskID == l.TaskID
                    && x.Date == l.Date);

                if (existing == null)
                {
                    DB.TasksLog.Add(l);
                }
            }

            //Comments
            foreach (var c in bundle.Comments)
            {
                c.OwnerId = tasks[c.OwnerId];               
                var existing = DB.Comments.FirstOrDefault(x =>
                    x.OwnerId == c.OwnerId
                    && x.Text == c.Text);

                if (existing == null)
                {
                    DB.Comments.Add(c);
                }
            }

        }
    }
}
