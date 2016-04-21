using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core.Lists;
using ToDo.Client.Core.Tasks;
using ToDo.Client.ViewModels;

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

            /// <summary>
            /// Get Lists and include TaskItems
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<TaskList> GetLists()
            {
                var query = DB.Lists
                    .Include(x => x.TaskItems);
                return query;
            }

            public static TaskItem GetTaskItem(int itemId)
            {
                var query = DB.Tasks
                    .Include(x => x.Parent)
                    .Include(x => x.Comments)
                    //.Include(x => x.Frequency) //TODO: Not needed as is enum
                    .Where(x => x.TaskItemID == itemId);

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
                return GetRootTaskItems(list.TaskListID);
            }

            public static ICollection<TaskItem> GetRootTaskItems(int listId)
            {
                var query = DB.Tasks
                    .Where(x => x.ParentID == null && x.ListID == listId)
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

                RenumberTasks(task.ListID, parentId);

            }

            public static void RenumberTasks(int listId, int? parentId)
            {
                ICollection<TaskItem> remaining = (from t in DB.Tasks
                                                   where t.ParentID == parentId
                                                   && t.ListID == listId
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

            public static bool MoveDown(TaskItem task)
            {
                if (task == null)
                    return false;
                else if (task.Order == GetNextOrder(task.List, task.Parent) - 1)
                    return false;

                TaskItem next = DB.Tasks.FirstOrDefault(x =>
                    x.Order == task.Order + 1
                    && x.ParentID == task.ParentID
                    );
                next.Order -= 1;
                task.Order += 1;

                DB.SaveChanges();

                return true;
            }

            public static void LoadList(int listId, ObservableCollection<TaskItemViewModel> collection, int? selected = null)
            {
                collection.Clear();

                var root = Workspace.API.GetRootTaskItems(listId);
                foreach (var t in root)
                {
                    collection.Add(new TaskItemViewModel(t));
                }

                if (selected != null)
                    Expand(collection, selected.Value);

            }

            private static bool Expand(IEnumerable<TaskItemViewModel> tasks, int id)
            {
                foreach (var t in tasks)
                {
                    if (t.Data.TaskItemID == id)
                    {
                        TaskItemViewModel parent = t.Parent;

                        while (parent != null)
                        {
                            parent.IsExpanded = true;
                            parent = parent.Parent;
                        }

                        return true;
                    }
                    else if (t.Children != null)
                    {
                        bool expanded = Expand(t.Children, id);
                        if (expanded)
                            return true;
                    }
                }

                return false;
            }

            public static bool MoveUp(TaskItem task)
            {
                if (task == null || task.Order == 1)
                    return false;

                TaskItem prev = DB.Tasks.FirstOrDefault(x =>
                    x.ParentID == task.ParentID
                    && x.Order == task.Order - 1);

                prev.Order += 1;
                task.Order -= 1;

                DB.SaveChanges();

                return true;
            }

            public static DateTime CalculateLastReminder(TaskFrequency frequency, DateTime referenceDate)
            {
                DateTime dt = referenceDate;

                switch (frequency)
                {
                    case TaskFrequency.Daily:
                        dt = dt.AddDays(-1);
                        break;
                    case TaskFrequency.Monthly:
                        dt = dt.AddMonths(-1);
                        break;
                    case TaskFrequency.Weekly:
                        dt = dt.AddDays(-7);
                        break;
                }

                return dt;
            }


            public static DateTime CalculateNextReminder(TaskFrequency frequency, DateTime referenceDate)
            {
                DateTime dt = referenceDate;

                while (dt <= DateTime.Today)
                {
                    switch (frequency)
                    {
                        case TaskFrequency.Daily:
                            dt = dt.AddDays(1);
                            break;
                        case TaskFrequency.Monthly:
                            dt = dt.AddMonths(1);
                            break;
                        case TaskFrequency.Weekly:
                            dt = dt.AddDays(7);
                            break;
                    }
                }

                return dt;
            }

            public static IEnumerable<TaskLog> GetTaskLogs(DateTime start, DateTime end)
            {
                return (from i in Instance.TasksLog
                        where i.Date >= start
                            && i.Date < end
                        select i).ToList();
            }

            public static void RemoveTaskLog(DateTime start, DateTime end, int taskItemId)
            {
                var items = GetTaskLogs(start, end).Where(x => x.TaskID == taskItemId);

                /*
                List<TaskLog> items =  (
                        from i in Instance.TasksLog
                        where i.Date >= start
                            && i.Date <= end
                            && i.TaskID == taskItemId
                        select i).ToList();
                */

                foreach (TaskLog i in items)
                {
                    Instance.TasksLog.Remove(i);
                }

                Instance.SaveChanges();
            }

            public static void MarkIncomplete(TaskItem task)
            {
                task.Completed = null;
                task.Updated = DateTime.Now;
                Workspace.instance.SaveChanges();

                //TODO: If Repeat, remove log if entry is between NextReminder - Frequency and NextReminder
                //Need to store Prev reminder?

                if (task.Frequency != null)
                {
                    var last = CalculateLastReminder(task.Frequency.Value, task.NextReminder.Value);
                    RemoveTaskLog(last, task.NextReminder.Value, task.TaskItemID);
                    
                }
            }
            public static void MarkCompleted(TaskItem task, DateTime completed)
            {
                task.Completed = completed;
                task.Updated = DateTime.Now;

                Workspace.Instance.SaveChanges();

                //TODO: Check for existing, update NextReminder?

                if (task.Frequency != null)
                {
                    TaskLog log = new TaskLog()
                    {
                        Date = completed,
                        TaskID = task.TaskItemID,
                        Completed = true
                    };

                    Instance.TasksLog.Add(log);

                    task.NextReminder = Workspace.API.CalculateNextReminder(task.Frequency.Value, task.NextReminder.Value);

                    Instance.SaveChanges();
                }
            }
        }
    }
}
