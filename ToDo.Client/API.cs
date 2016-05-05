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
using ToDo.Client.Core;

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

            #region Task Lists

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

            public static void UpdateList(TaskList existing, string title, string description)
            {
                ValidateTaskList(title, description, existing.TaskListID);

                existing.Title = title;
                existing.Description = description;
                existing.LastUpdated = DateTime.Now;

                DB.SaveChanges();
            }
            
            public static void InsertList(string name, string description, ListType type)
            {
                ValidateTaskList(name, description, type: type);

                var data = new TaskList(name, description, type);
                data.Created = data.LastUpdated = DateTime.Now;

                DB.Lists.Add(data);
                DB.SaveChanges();
            }

            private static void ValidateTaskList(string title, string description, int? id = null, ListType? type = null)
            {
                if (string.IsNullOrEmpty(title))
                    throw new Exception("Name cannot be empty.");

                var duplicate = (from l in Workspace.Instance.Lists
                                 where l.Title == title
                                 select l).FirstOrDefault();

                if (duplicate != null)
                {
                    if (!id.HasValue || duplicate.TaskListID != id.Value)
                        throw new Exception("A list by the same name already exists.");
                }

                if (id == null && type == null)
                    throw new Exception("Project Type must be defined.");
            }

            public static void DeleteList(TaskList list)
            {
                DB.Lists.Remove(list);
                DB.SaveChanges();
            }
            
            #endregion

            #region TaskItems

            public static void MarkIncomplete(TaskItem task)
            {
                task.Completed = null;
                task.Updated = DateTime.Now;
                Workspace.instance.SaveChanges();
            }
            public static void MarkCompleted(TaskItem task, DateTime completed)
            {
                task.Completed = completed;
                task.Updated = DateTime.Now;
                Workspace.Instance.SaveChanges();

                /*
                Logic handled by DateRoll Updater

                //TODO: Check for existing, update NextReminder?
                if (task.Frequency != null)
                {
                    var last = CalculateLastReminder(task.Frequency.Value, task.NextReminder.Value);
                    RemoveTaskLog(last, task.NextReminder.Value, task.TaskItemID);

                    TaskLog log = new TaskLog()
                    {
                        Date = completed,
                        TaskID = task.TaskItemID,
                        Completed = true
                    };

                    Instance.TasksLog.Add(log);

                    //task.NextReminder = Workspace.API.CalculateNextReminder(task.Frequency.Value, task.NextReminder.Value);

                    Instance.SaveChanges();
                }
                */
            }

            public static TaskItem GetTaskItem(int itemId)
            {
                var query = DB.Tasks
                    .Include(x => x.Parent)
                    .Include(x => x.Comments)
                    .Include(x => x.Children)
                    .Where(x => x.TaskItemID == itemId);

                var item = query.FirstOrDefault();

                return item;
            }
            
            public static List<TaskItem> GetRootTaskItems(TaskList list)
            {
                return GetRootTaskItems(list.TaskListID);
            }

            public static List<TaskItem> GetRootTaskItems(int listId)
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
                int? parentId = task.ParentID;
                int listId = task.ListID;

                DeleteChildren(task.Children);

                DB.Tasks.Remove(task);
                DB.SaveChanges();

                RenumberTasks(task.ListID, parentId);

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

            public static Stats GetStats()
            {
                int completed, remaining, overdue, total;

                var query = DB.Tasks.AsQueryable();
                total = query.Count();

                query = query.Where(x => x.Completed == null).AsQueryable();
                remaining = query.Count();
                completed = total - remaining;

                query = query.Where(x => x.DueDate != null && x.DueDate < DateTime.Today);
                overdue = query.Count();

                return new Stats(total, completed, remaining, overdue);
            }


            /// <summary>
            /// Returns the next order number for a new TaskItem
            /// </summary>
            /// <param name="list"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
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
            
            //TODO?: Add Order to TaskList and change both to use IOrderable (expose the Order property)
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

            private static bool HasTaskParentChanged(TaskItem item, TaskItem parent)
            {
                if (item == null)
                    return false;
                else if (item.Parent == null && parent == null)
                    return false;
                else if (item.Parent != null && parent != null && item.ParentID == parent.TaskItemID)
                    return false;
                else
                    return true;
            }

            public static void InsertTask(TaskList list, string title, string details, int? priority, 
                ICollection<Comment> comments,
                TaskItem parent, 
                TaskFrequency frequency, DateTime? dueDate, DateTime? completed)
            {   
                ValidateTaskValues(title, priority, frequency, dueDate);

                TaskItem item = new TaskItem();
                item.List = list;
                item.Title = title;
                item.Description = details;
                item.Priority = priority.Value;
                item.Parent = parent;
                item.Order = Workspace.API.GetNextOrder(list, parent);
                //item.DueDate = dueDate;
                item.Created = item.Updated = DateTime.Now;
                SetFrequency(item, frequency, dueDate);
                
                Workspace.Instance.Tasks.Add(item);
                Workspace.Instance.SaveChanges();

                SetCompleted(item, completed);

                foreach (var comment in comments)
                {
                    comment.Owner = item;
                    DB.Comments.Add(comment);
                }

                DB.SaveChanges();
            }

            public static void UpdateTask(TaskItem existing, string title, string details, ICollection<Comment> comments, int? priority, TaskItem parent, int order, TaskFrequency frequency, DateTime? dueDate, DateTime? completed)
            {
                ValidateTaskValues(title, priority, frequency, dueDate, existing.TaskItemID);

                existing.Title = title;
                existing.Description = details;
                existing.Priority = priority.Value;
                existing.Updated = DateTime.Now;
                SetFrequency(existing, frequency, dueDate);

                bool parentChanged = HasTaskParentChanged(parent, existing.Parent);
                int? oldParentId = existing.ParentID; //Used for renumbering further below

                if (parentChanged)
                {
                    existing.Order = Workspace.API.GetNextOrder(existing.List, parent);
                    existing.Parent = parent;
                }

                
                Workspace.Instance.SaveChanges();

                //Impacts DB directly/immediately

                if (parentChanged)
                {
                    int listId = existing.ListID;

                    Workspace.API.RenumberTasks(listId,
                        oldParentId);

                    Workspace.API.RenumberTasks(listId,
                        existing.ListID);
                }

                SetCompleted(existing, completed);

                //Process Comments

                foreach (var c in comments.Where(x => x.Owner == null))
                    c.Owner = existing;

                ProcessComments(existing.Comments, comments);
            }

            /// <summary>
            /// Copy new comments list to existing, find mismatchs (Deletes/Adds)
            /// </summary>
            /// <param name="existing"></param>
            /// <param name="updated"></param>
            private static void ProcessComments(ICollection<Comment> existing, ICollection<Comment> updated)
            {
                var ex = existing == null? new List<Comment>() : existing.OrderByDescending(x => x.CommentID).ToList();
                var up = updated == null? new List<Comment>() : updated.OrderByDescending(x => x.CommentID).ToList();

                int p1 = 0, p2 = 0;

                Comment c1, c2;

                List<Comment> add = new List<Comment>();
                List<Comment> delete = new List<Comment>();

                while (p1 < ex.Count && p2 < up.Count)
                {
                    c1 = ex[p1];
                    c2 = up[p2];

                    if (c1.CommentID == c2.CommentID)
                    {
                        c1.Text = c2.Text;
                        p1++;
                        p2++;
                    }
                    else //Not matched -> deleted
                    {
                        delete.Add(c1);
                        p1++;
                    }
                }

                while (p2 < up.Count)
                {
                    add.Add(up[p2]);
                    p2++;
                }

                while (p1 < ex.Count)
                {
                    delete.Add(ex[p1]);
                    p1++;
                }

                foreach (var c in delete)
                {
                    DB.Comments.Remove(c);
                }

                foreach (var c in add)
                {
                    DB.Comments.Add(c);
                }

                DB.SaveChanges();
                
            }

            /// <summary>
            /// Marks the Task as completed. Affects DB and item directly
            /// </summary>
            /// <param name="task"></param>
            /// <param name="completed"></param>
            private static void SetCompleted(TaskItem task, DateTime? completed)
            {
                if (completed != null)
                {
                    Workspace.API.MarkCompleted(task, completed.Value);
                }
                else
                    Workspace.API.MarkIncomplete(task);
            }

            private static void SetFrequency(TaskItem task, TaskFrequency frequency, DateTime? startDate)
            {
                if (frequency != TaskFrequency.No && startDate != null)
                {
                    DateTime reminder = GetNextReminder(frequency, startDate.Value);
                    task.Frequency = frequency;
                    task.DueDate = startDate;
                }
                else //Delete
                {
                    task.Frequency = TaskFrequency.No;
                    task.DueDate = null;
                }
            }

            private static DateTime GetNextReminder(TaskFrequency frequency, DateTime startDate)
            {
                if (startDate >= DateTime.Today)
                    return startDate;
                else
                    return Workspace.API.CalculateNextReminder(frequency, startDate);                
            }

            private static void ValidateTaskValues(string title, int? priority,
                TaskFrequency? frequency, DateTime? startDate,
                int? id = null)
            {
                //Title
                var existing = Workspace.Instance.Tasks.FirstOrDefault(x => x.Title == title);
                if (existing != null)
                {
                    if (id == null || existing.TaskItemID != id)
                    {
                        throw new Exception("An task by this name already exists.");
                    }
                }

                //Priority
                if (priority == null)
                    throw new Exception("Priority must be set.");

                //Frequency
                if ((frequency == null && startDate != null)
                    || (frequency != null && startDate == null))
                    throw new Exception("Frequency and DueDate must both be set or unset.");
            }

            #endregion
            
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

                        t.Selected = true;

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
            
            #region Reminders
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

                while (dt < DateTime.Today)
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

            #endregion

            #region Task Logging

            public static IEnumerable<TaskLog> GetTaskLogs(DateTime start, DateTime end)
            {
                return (from i in Instance.TasksLog
                        where i.Date >= start
                            && i.Date < end
                        select i).ToList();
            }

            /*
            //May not be needed anymore as tasks are not logged until after date rolls
            public static void RemoveTaskLog(DateTime start, DateTime end, int taskItemId)
            {
                var items = GetTaskLogs(start, end).Where(x => x.TaskID == taskItemId);

                foreach (TaskLog i in items)
                {
                    Instance.TasksLog.Remove(i);
                }

                Instance.SaveChanges();
            }
            */

            #endregion

        }
    }
}
