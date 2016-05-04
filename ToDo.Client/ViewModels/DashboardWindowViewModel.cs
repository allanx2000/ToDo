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
using ToDo.Client.Core.Tasks;

namespace ToDo.Client.ViewModels
{
    class DashboardWindowViewModel : ViewModel
    {
        private Window window;

        private CollectionViewSource listsViewSource;
        private ObservableCollection<TaskListViewModel> lists = new ObservableCollection<TaskListViewModel>();

        private CollectionViewSource tasksViewSource;
        private ObservableCollection<TaskItemViewModel> tasks = new ObservableCollection<TaskItemViewModel>();
        
        public DashboardWindowViewModel(Window window)
        {
            this.window = window;

            listsViewSource = new CollectionViewSource();
            listsViewSource.Source = lists;
            listsViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            tasksViewSource = new CollectionViewSource();
            tasksViewSource.Source = tasks;
            //tasksViewSource.SortDescriptions.Add(new SortDescription("IsComplete", ListSortDirection.Ascending));
            tasksViewSource.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));

            ReloadLists();
            UpdateStats();

            TasksUpdateTimer.OnTasksUpdated += TasksUpdateTimer_OnTasksUpdated;
            TasksUpdateTimer.UpdateTasks();
            TasksUpdateTimer.StartTimer();
        }
        
        private void TasksUpdateTimer_OnTasksUpdated()
        {
            foreach (var t in tasks)
            {
                t.RefreshViewModel();
            }

            foreach (var l in lists)
            {
                l.Update();
            }
        }

        #region Lists

        private void ReloadLists()
        {
            lists.Clear();

            foreach (var list in Workspace.API.GetLists())
            {
                lists.Add(new TaskListViewModel(list));
            }
        }

        private TaskListViewModel selectedList;
        public TaskListViewModel SelectedList
        {
            get
            {
                return selectedList;
            }
            set
            {
                selectedList = value;
                RaisePropertyChanged();

                LoadTasks();
            }
        }

        
        public ICollectionView Lists
        {
            get
            {
                return listsViewSource.View;
            }
        }

        public ICommand DeleteListCommand
        {
            get { return new CommandHelper(DeleteList); }
        }

        private void DeleteList()
        {
            if (SelectedList == null)
                return;

            if (MessageBoxFactory.ShowConfirmAsBool("Are you sure you want to delete: " + SelectedList.Name, "Delete List"))
            {
                Workspace.API.DeleteList(SelectedList.Data);
                ReloadLists();
            }
        }

        public ICommand AddListCommand
        {
            get
            {
                return new CommandHelper(AddList);
            }
        }

        private void AddList()
        {
            EditListWindow window = new EditListWindow();
            window.ShowDialog();

            if (window.Cancelled)
            {
                return;
            }
            else
                ReloadLists();
        }

        public ICommand EditListCommand
        {
            get
            {
                return new CommandHelper(EditList);
            }
        }

        private void EditList()
        {
            if (SelectedList == null)
                return;

            EditListWindow window = new EditListWindow(SelectedList);
            window.ShowDialog();

            if (window.Cancelled)
            {
                return;
            }
            else
                ReloadLists();
        }

        #endregion

        #region Tasks
        
        public ICollectionView Tasks
        {
            get
            {
                return tasksViewSource.View;
            }
        }

        public Visibility DetailsVisible
        {
            get
            {
                return SelectedTaskViewModel == null ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private TaskItemViewModel selectedTaskViewModel;
        public TaskItemViewModel SelectedTaskViewModel
        {
            get { return selectedTaskViewModel; }
            set
            {
                selectedTaskViewModel = value;
                RaisePropertyChanged();
                RaisePropertyChanged("SelectedTask");
                RaisePropertyChanged("DetailsVisible");

            }
        }

        //private TaskItem selectedTask;
        public TaskItem SelectedTask
        {
            get { return SelectedTaskViewModel == null ? null : SelectedTaskViewModel.Data; } //selectedTask; 
            /*
            set
            {
                selectedTask = value;
                RaisePropertyChanged();

                //UpdateDetails();
            }*/

        }

        private void UpdateDetails()
        {
            //throw new NotImplementedException();
        }

        public ICommand AddSubTaskCommand
        {
            get
            {
                return new CommandHelper(AddSubTask);
            }
        }

        private void AddSubTask()
        {
            if (SelectedTask == null || SelectedList == null)
                return;

            var window = new EditTaskWindow(SelectedList.Data, SelectedTask);
            window.ShowDialog();

            if (!window.Cancelled)
            {
                LoadTasks();
            }
        }

        public ICommand EditTaskCommand
        {
            get
            {
                return new CommandHelper(EditTask);
            }
        }

        private void EditTask()
        {
            if (SelectedTask == null || SelectedList == null)
                return;

            var window = new EditTaskWindow(SelectedTask); //TODO: Check if filled
            window.ShowDialog();

            if (!window.Cancelled)
            {
                LoadTasks();
            }
        }

        public ICommand AddTaskCommand
        {
            get
            {
                return new CommandHelper(AddTask);
            }
        }

        private void AddTask()
        {
            if (SelectedList == null)
                return;

            var window = new EditTaskWindow(SelectedList.Data);
            window.ShowDialog();

            if (!window.Cancelled)
            {
                LoadTasks();
            }

        }

        private void LoadTasks()
        {
            if (selectedList == null)
                return;

            int? prevTask = SelectedTask == null ? null : (int?)SelectedTask.TaskItemID;

            Workspace.API.LoadList(SelectedList.Data.TaskListID, tasks, prevTask);

            SelectedList.Update();

            UpdateStats();

        }

        public int TotalCompleted { get; private set; }
        public int TotalRemaining { get; private set; }
        public int TotalOverdue { get; private set; }


        private void UpdateStats()
        {
            Stats stats = Workspace.API.GetStats();
            TotalCompleted = stats.Completed;
            TotalRemaining = stats.Remaining;
            TotalOverdue = stats.Overdue;

            RaisePropertyChanged("TotalCompleted");
            RaisePropertyChanged("TotalRemaining");
            RaisePropertyChanged("TotalOverdue");
        }

        private bool Expand(IEnumerable<TaskItemViewModel> tasks, int id)
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

        public ICommand MarkCompletedCommand
        {
            get
            {
                return new CommandHelper(MarkCompleted);
            }
        }

        public void MarkCompleted()
        {
            try
            {
                if (SelectedTask != null && SelectedTask.Completed == null)
                {
                    Workspace.API.MarkCompleted(SelectedTask, DateTime.Today);
                    
                    LoadTasks();
                }
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        public ICommand DeleteItemCommand
        {
            get
            {
                return new CommandHelper(DeleteItem);
            }
        }

        private void DeleteItem()
        {
            try
            {
                if (SelectedTask == null)
                    return;
                else if (!MessageBoxFactory.ShowConfirmAsBool("Are you sure you want to delete this Task and ALL it's SubTasks?",
                    "Confirm Delete",
                    System.Windows.MessageBoxImage.Exclamation))
                    return;

                var parent = SelectedTask.Parent;

                /*
                FIXME: Needs to be changed to ViewModel
                if (parent != null)
                    SelectedTask = parent;
                */

                Workspace.API.DeleteTask(SelectedTask);

                LoadTasks();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        public ICommand MoveUpCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    bool moved = Workspace.API.MoveUp(SelectedTask);

                    if (moved)
                        LoadTasks();
                });
            }
        }

        public ICommand MoveDownCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    bool moved = Workspace.API.MoveDown(SelectedTask);

                    if (moved)
                        LoadTasks();
                });
            }
        }

        #endregion

        //#region Task Details


        #region Misc

        public ICommand ChangeDBCommand
        {
            get { return new CommandHelper(ChangeDB); }
        }

        private void ChangeDB()
        {
            if (!MessageBoxFactory.ShowConfirmAsBool("Are you sure you want to close the current database?", "Close Database?", MessageBoxImage.Asterisk))
                return;

            Workspace.Unload();

            LoadWindow loader = new LoadWindow();
            loader.Show();

            window.Close();
        }

        public ICommand ExportImportCommand
        {
            get
            {
                return new CommandHelper(ShowExportImportWindow);
            }
        }
        
        private void ShowExportImportWindow()
        {
            var dlg = new ExportImportWindow();
            dlg.ShowDialog();
        }

        #endregion
    }
}
