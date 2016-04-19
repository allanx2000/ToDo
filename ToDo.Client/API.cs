using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core.Lists;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client
{
    public partial class Workspace : DbContext
    {
        public static class API
        {
            public static Workspace DB
            {
                get
                {
                    return Workspace.Instance;
                }
            }

            public static IEnumerable<TaskList> GetLists()
            {
                var query = DB.Lists
                    .Include(x => x.TaskItems);
                return query;
            }

            public static TaskItem GetTaskItem(int id)
            {
                var query = DB.Tasks
                    .Include(x => x.Parent)
                    //.Include(x => x.Children)
                    .Include(x => x.Comments)
                    //.Include(x => x.Frequency) TODO: Add back, FK removed?
                    .Where(x => x.TaskItemID == id);

                var item = query.FirstOrDefault();

                //Add Children
                if (item != null)
                {
                    var children = DB.Tasks.Where(x => x.ParentID == item.TaskItemID).ToList();

                    if (children.Count > 0)
                    {
                        item.Children = new List<TaskItem>();

                        foreach (var i in children)
                        {
                            item.Children.Add(i);
                        }
                    }
                }

                return item;
            }

            public static ICollection<TaskItem> GetRootTaskItems(TaskList list)
            {
                var query = DB.Tasks
                    .Where(x => x.ParentID == null && x.ListID == list.TaskListID)
                    .OrderBy(x => x.Order);

                List<TaskItem> tasks = new List<TaskItem>();

                foreach (var t in query.ToList())
                {
                    tasks.Add(GetTaskItem(t.TaskItemID));
                }

                return tasks;
            }

            public static void DeleteTask(TaskItem task)
            {
                //List<TaskItem> children = new List<TaskItem>();
                int? parentId = task.ParentID;
                int listId = task.ListID;

                DeleteChildren(task.Children);

                DB.Tasks.Remove(task);
                DB.SaveChanges();

                ICollection<TaskItem> remaining;

                remaining = (from t in DB.Tasks
                             where t.ParentID == parentId
                             orderby t.Order ascending
                             select t).ToList();

                int ctr = 1;
                foreach (var t in remaining)
                {
                    t.Order = ctr++;
                }

                DB.SaveChanges();

            }

            private static void DeleteChildren(ICollection<TaskItem> children)
            {
                if (children == null)
                    return;

                foreach (var c in children)
                {
                    DeleteChildren(c.Children);

                    DB.Tasks.Remove(c);
                }

                DB.SaveChanges();
            }

            public static int GetNextOrder(TaskList list, TaskItem parent)
            {
                int order;
                if (parent != null)
                    order = (from t in DB.Tasks
                             where t.ParentID == parent.TaskItemID
                             select t.Order).Max();
                else
                    order = (from t in DB.Tasks
                             where t.ListID == list.TaskListID
                             select t.Order).Max();
                return order + 1;
            }
        }
    }
}
