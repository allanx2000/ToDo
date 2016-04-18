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
        static List<int> priorities;

        private TaskItem parent;
        private TaskItem existing;
        private Window window;

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
                Completed = existing.Completed != null;

                Priority = existing.Priority;

                DueDate = existing.DueDate;
                HasDueDate = DueDate != null;

                HasRepeat = existing.Frequency != null;
                if (HasRepeat)
                    SelectedFrequency = existing.Frequency.ToString();

                SetParent(existing.Parent);

            }
        }

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

        private int? priority = null;
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
                if (priorities == null)
                {
                    priorities = new List<int>();

                    for (int i = 1; i <= 5; i++)
                        priorities.Add(i);
                }

                return priorities;
            }
        }

        public string ParentText
        {
            get
            {
                return parent == null ? "" : parent.Title;
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

        public bool Cancelled { get; private set; }

        private bool completed;
        private TaskList list;

        public bool Completed
        {
            get { return completed; }
            set
            {
                //TODO: Need to add a DatePicker?
                completed = value;
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

        public ICommand SaveCommand
        {
            get
            {
                return new CommandHelper(Save);
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
                    item.Created = now;

                item.Title = Name;
                item.Description = Details;

                if (Completed)
                {
                    if (item.Completed == null)
                        item.Completed = now;
                }
                else
                    item.Completed = null;

                item.Priority = Priority.Value;

                if (HasDueDate)
                    item.DueDate = DueDate;

                if (parent != null)
                {
                    item.Parent = parent;
                }

                item.Updated = now;

                if (HasRepeat)
                {

                }
                else //Delete
                {

                }

                if (!isExisting)
                {
                    item.List = list;
                    Workspace.Instance.Tasks.Add(item);
                }

                Workspace.Instance.SaveChanges();

                Cancelled = false;
                window.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }
    }
}
