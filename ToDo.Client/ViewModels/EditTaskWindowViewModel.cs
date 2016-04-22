using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ToDo.Client.Core.Lists;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client.ViewModels
{
    public class EditTaskWindowViewModel : ViewModel
    {
        static List<int> PrioritiesList;

        private TaskItem parent;
        private TaskItem existing;

        private TaskList list;

        private Window window;

        public bool Cancelled { get; private set; }

        private void Initialize(Window window)
        {
            Cancelled = true;
            this.window = window;
        }

        /// <summary>
        /// Edit Existing
        /// </summary>
        /// <param name="window"></param>
        /// <param name="existing"></param>
        public EditTaskWindowViewModel(Window window, TaskItem existing)
        {
            Initialize(window);
            LoadExisting(existing);
        }

        public EditTaskWindowViewModel(Window window, TaskList list)
        {
            Initialize(window);
            this.list = list;
        }

        public void SetParent(TaskItem parent)
        {
            this.parent = parent;
            RaisePropertyChanged("ParentText");
        }

        private void LoadExisting(TaskItem item)
        {
            existing = item;

            if (existing != null)
            {
                Name = existing.Title;
                Details = existing.Description;
                //TODO: Add Edit Comments row | existing.Comments
                HasCompleted = existing.Completed != null;
                Completed = existing.Completed;

                Priority = existing.Priority;

                DueDate = existing.DueDate;
                HasDueDate = DueDate != null;

                StartDate = existing.StartDate;

                HasRepeat = existing.Frequency != null;
                if (HasRepeat)
                    SelectedFrequency = existing.Frequency.ToString();

                SetParent(existing.Parent);
            }
        }

        #region Properties and Commands

        private string selectedFrequency;
        public string SelectedFrequency
        {
            get { return selectedFrequency; }
            set
            {
                selectedFrequency = value;
                RaisePropertyChanged();
            }
        }

        private static List<string> frequencies;
        public List<string> Frequencies
        {
            get
            {
                if (frequencies == null)
                {
                    frequencies = new List<string>();

                    foreach (var val in Enum.GetValues(typeof(TaskFrequency)))
                    {
                        frequencies.Add(val.ToString());
                    }
                }

                return frequencies;
            }
        }

        private bool hasRepeat;
        public bool HasRepeat
        {
            get { return hasRepeat; }
            set
            {
                hasRepeat = value;

                if (!hasRepeat)
                    SelectedFrequency = null;

                RaisePropertyChanged();
            }
        }

        private int? priority = 3;
        public int? Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
                RaisePropertyChanged();
            }
        }

        public List<int> Priorities
        {
            get
            {
                if (PrioritiesList == null)
                {
                    PrioritiesList = new List<int>();

                    for (int i = 1; i <= 5; i++)
                        PrioritiesList.Add(i);
                }

                return PrioritiesList;
            }
        }

        public string ParentText
        {
            get
            {
                return parent == null ? "(None)" : parent.Title;
            }
        }

        private bool hasDueDate;
        public bool HasDueDate
        {
            get
            {
                return hasDueDate;
            }
            set
            {
                hasDueDate = value;

                if (hasDueDate == false)
                    DueDate = null;

                RaisePropertyChanged();
            }
        }

        private DateTime? dueDate;
        public DateTime? DueDate
        {
            get
            {
                return dueDate;
            }
            set
            {
                dueDate = value;
                RaisePropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                return (existing != null ? "Edit" : "Add") + " Task";
            }
        }

        public string name, details;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        public string Details
        {
            get
            {
                return details;
            }
            set
            {
                details = value;
                RaisePropertyChanged();
            }
        }



        private DateTime? completed;
        public DateTime? Completed
        {
            get
            {
                return completed;
            }
            set
            {
                completed = value;
                RaisePropertyChanged();
            }
        }

        private bool hasCompleted;

        public bool HasCompleted
        {
            get { return hasCompleted; }
            set
            {
                //TODO: Need to add a DatePicker?
                hasCompleted = value;

                if (!hasCompleted)
                    Completed = null;
                else if (Completed == null)
                    Completed = DateTime.Today;

                RaisePropertyChanged();
            }
        }
        public ICommand CancelCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    Cancelled = true;
                    window.Close();
                });
            }
        }

        public ICommand SelectParentCommand
        {
            get
            {
                return new CommandHelper(SelectParent);
            }
        }

        private void SelectParent()
        {
            int listId = existing == null ? list.TaskListID : existing.ListID;
            int? currentTaskId = existing == null ? null : (int?)existing.TaskItemID;

            var window = new ParentSelectWindow(listId, currentTaskId);
            window.ShowDialog();

            if (!window.Cancelled)
            {
                if (window.SelectedItem == null)
                    SetParent(null);
                else
                    SetParent(window.SelectedItem.Data);
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new CommandHelper(Save);
            }
        }

        private DateTime? startDate;

        public DateTime? StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                RaisePropertyChanged();
            }
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrEmpty(Name))
                    throw new Exception("Name cannot be empty.");
                else if (Priority == null)
                    throw new Exception("Priority is required.");
                else if (HasCompleted && Completed == null)
                    throw new Exception("Completed Date not set.");
                else if (HasRepeat)
                {
                    if (string.IsNullOrEmpty(SelectedFrequency))
                        throw new Exception("No Frequency selected.");
                    else if (StartDate == null)
                        throw new Exception("No Start Date set.");
                }
                else if (HasDueDate)
                {
                    if (DueDate == null)
                        throw new Exception("Due Date not set.");
                }
                

                var sameName = Workspace.Instance.Tasks.FirstOrDefault(x => x.Title == Name);
                if (sameName == null || (existing != null && sameName.TaskItemID == existing.TaskItemID))
                {
                    //OK
                }
                else
                    throw new Exception("A task with the name already exists.");

                DateTime now = DateTime.Now;

                bool isExisting = existing != null;
                TaskItem item = isExisting ? existing : new TaskItem();

                if (!isExisting)
                {
                    item.Created = now;
                    item.Order = Workspace.API.GetNextOrder(list, parent);
                }

                item.Title = Name;
                item.Description = Details;


                item.Priority = Priority.Value;
                
                int? oldParentId = null;
                bool parentChanged = ParentChanged(item, parent);
                if (parentChanged)
                {
                    oldParentId = item.ParentID;

                    item.Order = Workspace.API.GetNextOrder(list, parent);
                    item.Parent = parent;

                }

                item.Updated = now;

                if (HasRepeat)
                {
                    item.Frequency = (TaskFrequency)Enum.Parse(typeof(TaskFrequency), SelectedFrequency);
                    item.StartDate = StartDate;

                    if (item.NextReminder == null 
                        || item.NextReminder.Value < startDate)
                    {
                        if (startDate >= DateTime.Today)
                            item.NextReminder = startDate;
                        else
                            item.NextReminder = Workspace.API.CalculateNextReminder(item.Frequency.Value, StartDate.Value);
                    }
                }
                else //Delete
                {
                    item.Frequency = null;
                    item.StartDate = null;
                    item.NextReminder = null;
                }

                if (HasDueDate)
                {
                    item.DueDate = DueDate;
                }
                else
                    item.DueDate = null;

                if (!isExisting) //Set ListId
                {
                    item.List = list;
                    Workspace.Instance.Tasks.Add(item);
                }

                Workspace.Instance.SaveChanges();

                //Impacts DB directly/immediately

                if (parentChanged)
                {
                    int listId = existing == null ? list.TaskListID : existing.ListID;
                    Workspace.API.RenumberTasks(listId,
                        oldParentId);

                    Workspace.API.RenumberTasks(listId,
                        item.ListID);
                }

                if (HasCompleted)
                {
                    Workspace.API.MarkCompleted(item, Completed.Value);
                }
                else
                    Workspace.API.MarkIncomplete(item);

                Cancelled = false;
                window.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        private bool ParentChanged(TaskItem item, TaskItem parent)
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

        #endregion
    }
}
