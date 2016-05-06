using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ToDo.Client.Core;
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

            commentsSource = new CollectionViewSource();
            commentsSource.Source = comments;
            commentsSource.SortDescriptions.Add(new SortDescription("Created", ListSortDirection.Ascending));
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
            if (existing == null)
                return;
            
            Name = existing.Title;
            Details = existing.Description;
            Priority = existing.Priority;

            HasCompleted = existing.Completed != null;
            Completed = existing.Completed;
            
            DueDate = existing.DueDate;
            HasDueDate = existing.DueDate != null;

            SelectedFrequency = existing.Frequency.ToString();

            SetParent(existing.Parent);

            if (existing.Comments != null)
            {
                foreach (Comment c in existing.Comments)
                {
                    comments.Add(new CommentViewModel(c));
                }
            }
            else comments = new ObservableCollection<CommentViewModel>();
        }
        
        #region Properties

        /// <summary>
        /// Window Title
        /// </summary>
        public string Title
        {
            get
            {
                return (existing != null ? "Edit" : "Add") + " Task";
            }
        }

        #region Comments

        private CollectionViewSource commentsSource;
        private ObservableCollection<CommentViewModel> comments = new ObservableCollection<CommentViewModel>();

        public ICollectionView CommentsView
        {
            get
            {
                return commentsSource.View;
            }
        }

        private CommentViewModel selectedComment;
        public CommentViewModel SelectedComment
        {
            get { return selectedComment; }
            set
            {
                selectedComment = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Frequency/Repeat

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
        /*
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
        */

        #endregion

        #region DueDate

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
                else
                    dueDate = DateTime.Today;

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

        #endregion

        #region Priority, Name, Details
        
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

        #endregion

        #region Completed

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

        #endregion

        #endregion

        #region Commands

        #region Comments

        public ICommand DeleteCommentCommand
        {
            get { return new CommandHelper(DeleteComment); }
        }

        private void DeleteComment()
        {
            if (SelectedComment == null)
                return;

            comments.Remove(SelectedComment);
            SelectedComment = null;
        }

        public ICommand EditCommentCommand
        {
            get { return new CommandHelper(EditComment); }
        }

        private void EditComment()
        {
            if (SelectedComment == null)
                return;

            var ecw = new EditCommentWindow(SelectedComment);
            ecw.ShowDialog();

            if (!ecw.Cancelled)
            {
                comments.Remove(SelectedComment);
                comments.Add(new CommentViewModel(ecw.GetData()));
            }
        }


        public ICommand NewCommentCommand
        {
            get { return new CommandHelper(NewComment); }
        }

        private void NewComment()
        {
            EditCommentWindow ecw = new EditCommentWindow();
            ecw.ShowDialog();

            if (!ecw.Cancelled)
            {
                Comment comment = ecw.GetData();
                comments.Add(new CommentViewModel(comment));
            }
        }

        #endregion

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
        
        private void Save()
        {
            try
            {
                ValidateFields();

                bool isExisting = existing != null;
                

                ICollection<Comment> comments = GetComments(this.comments);
                if (!HasDueDate)
                    SelectedFrequency = null;

                if (isExisting)
                {
                    Workspace.API.UpdateTask(existing, Name, Details, comments, Priority,
                        parent,
                        existing.Order,
                        ConvertToFrequency(SelectedFrequency),
                        HasDueDate ? DueDate : null,
                        HasCompleted ? Completed : null);
                }
                else
                {
                    Workspace.API.InsertTask(list, Name, Details, Priority, 
                        comments,
                        parent,
                        ConvertToFrequency(SelectedFrequency),
                        HasDueDate ? DueDate : null,
                        HasCompleted ? Completed : null);
                }

                Cancelled = false;
                window.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        private ICollection<Comment> GetComments(ObservableCollection<CommentViewModel> comments)
        {
            List<Comment> results = new List<Comment>();

            foreach (var vm in comments)
                results.Add(vm.Data);

            return results;
        }

        private void ValidateFields()
        {
            if (string.IsNullOrEmpty(Name))
                throw new Exception("Name cannot be empty.");
            else if (Priority == null)
                throw new Exception("Priority is required.");
            else if (HasCompleted && Completed == null)
                throw new Exception("Completed Date not set.");
            else if (HasDueDate)
            {
                if (DueDate == null)
                    throw new Exception("Due Date not set.");
            }
        }

        private TaskFrequency ConvertToFrequency(string frequency)
        {
            if (string.IsNullOrEmpty(SelectedFrequency))
                return TaskFrequency.No;
            else
                return (TaskFrequency)Enum.Parse(typeof(TaskFrequency), SelectedFrequency);
        }
        
        #endregion

        /*
        Moved to API
        private void CheckName()
        {
            var sameName = Workspace.Instance.Tasks.FirstOrDefault(x => x.Title == Name);
            if (sameName == null || (existing != null && sameName.TaskItemID == existing.TaskItemID))
            {
                //OK
            }
            else
                throw new Exception("A task with the name already exists.");
        }

        private void ScheduleRepeat(TaskItem item)
        {
            item.Frequency = (TaskFrequency)Enum.Parse(typeof(TaskFrequency), SelectedFrequency);

            bool dateChanged = StartDate != item.StartDate;
            item.StartDate = StartDate;

            if (dateChanged)
                item.NextReminder = null;

            if (item.NextReminder == null
                || item.NextReminder.Value < startDate)
            {
                if (StartDate >= DateTime.Today)
                    item.NextReminder = StartDate;
                else
                    item.NextReminder = Workspace.API.CalculateNextReminder(item.Frequency.Value, StartDate.Value);
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
        */

    }
}
