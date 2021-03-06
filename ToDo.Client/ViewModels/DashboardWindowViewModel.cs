﻿using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using NHotkey.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ToDo.Client.Core.Tasks;
using NHotkey;

namespace ToDo.Client.ViewModels
{
    class DashboardWindowViewModel : ViewModel
    {
        private Window window;

        private CollectionViewSource listsViewSource;
        private ObservableCollection<TaskListViewModel> lists = new ObservableCollection<TaskListViewModel>();

        private CollectionViewSource tasksViewSource;
        private ObservableCollection<TaskItemViewModel> tasks = new ObservableCollection<TaskItemViewModel>();

        private ObservableCollection<TaskItemViewModel> quickList = new ObservableCollection<TaskItemViewModel>();
        private CollectionViewSource quickListSource;

        private Action onQuit;

        public DashboardWindowViewModel(Window window, Action onQuit)
        {
            this.window = window;
            this.onQuit = onQuit;

            InitializeViewSources();

            //Default Values
            SelectedListOrder = Remaining;
            SelectedQuickListType = ComingDue;

            Hotkey.SetDefaultShowWindowHotkey(ShowWindow);

            ReloadLists();
            UpdateStats();

            quickListSource = new CollectionViewSource();
            quickListSource.Source = quickList;

            TasksUpdateTimer.OnTasksUpdated += TasksUpdateTimer_OnTasksUpdated;
            TasksUpdateTimer.UpdateTasks();
            TasksUpdateTimer.StartTimer();
        }
        
        private void InitializeViewSources()
        {
            listsViewSource = new CollectionViewSource();
            listsViewSource.Source = lists;

            tasksViewSource = new CollectionViewSource();
            tasksViewSource.Source = tasks;
            SortDescriptions.SetSortDescription(tasksViewSource.SortDescriptions, SortDescriptions.TaskItemsOrder);

            quickListSource = new CollectionViewSource();
            quickListSource.Source = quickList;
        }

        private void TasksUpdateTimer_OnTasksUpdated()
        {
            App.Current.Dispatcher.Invoke(TasksChanged);
        }
        
        #region Lists

        #region Ordering
        public const string Alphabetical = "Alphabetical";
        public const string Type = "Type";
        public const string Remaining = "Remaining Tasks";

        private static readonly List<string> listOrder = new List<string>()
        {
            Alphabetical,
            Type,
            Remaining,
        };

        private string selectedListOrder;
        public string SelectedListOrder
        {
            get { return selectedListOrder; }
            set
            {
                selectedListOrder = value;
                RaisePropertyChanged();

                SetListOrder(selectedListOrder);
            }
        }

        public List<string> ListOrder
        {
            get { return listOrder; }
        }

        private readonly SortDescription alpha = new SortDescription("Name", ListSortDirection.Ascending);
        private readonly SortDescription type = new SortDescription("Type", ListSortDirection.Ascending);
        private void SetListOrder(string order)
        {
            listsViewSource.SortDescriptions.Clear();

            switch (order)
            {
                case Type:
                    listsViewSource.SortDescriptions.Add(type);
                    listsViewSource.SortDescriptions.Add(alpha);
                    break;
                case Remaining:
                    listsViewSource.SortDescriptions.Add(new SortDescription("Remaining", ListSortDirection.Descending));
                    listsViewSource.SortDescriptions.Add(alpha);
                    break;
                default:
                case Alphabetical:
                    listsViewSource.SortDescriptions.Add(alpha);
                    break;

            }

            RefreshListsView();
        }

        #endregion

        /// <summary>
        /// Loads all the Lists from DB, converting to ListViewModel
        /// Called on all list updates
        /// TODO: Maybe can just refresh existing viewmodels on info update, rather than reloading
        /// </summary>
        private void ReloadLists(bool refreshOnly = false)
        {
            if (refreshOnly)
            {
                foreach (var l in lists)
                    l.RefreshViewModel();
            }
            else {
                lists.Clear();

                foreach (var list in Workspace.API.GetLists())
                {
                    lists.Add(new TaskListViewModel(list));
                }
            }
            RaisePropertyChanged("Lists");
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
                ReloadLists(true);
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
                return SelectedTaskViewModel == null ? Visibility.Collapsed : Visibility.Visible;
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

        public TaskItem SelectedTask
        {
            get { return SelectedTaskViewModel == null ? null : SelectedTaskViewModel.Data; } //selectedTask; 

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
                TasksChanged();
            }
        }

        public ICommand EditTaskCommand
        {
            get
            {
                return new CommandHelper(() => EditTask(SelectedTask));
            }
        }

        private void EditTask(TaskItem task)
        {
            if (task == null)
                return;

            var window = new EditTaskWindow(task); //TODO: Check if filled
            window.ShowDialog();

            if (!window.Cancelled)
            {
                TasksChanged();
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
                TasksChanged();

            }

        }
        /// <summary>
        /// Updates all UI elements when a task is added, edited, or deleted
        /// </summary>
        private void TasksChanged()
        {
            LoadTasks();
            RefreshListsView();
            UpdateQuickList();
            UpdateStats();
        }

        private void RefreshQuickList()
        {
            QuickListView.Refresh();
        }

        private void LoadTasks()
        {
            if (selectedList == null)
                return;

            int? prevTask = SelectedTask == null ? null : (int?)SelectedTask.TaskItemID;

            Workspace.API.LoadList(SelectedList.Data.TaskListID, tasks, prevTask);

            SelectedList.Update();

        }

        private bool ExpandAndSelect(IEnumerable<TaskItemViewModel> tasks, int id)
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
                    bool expanded = ExpandAndSelect(t.Children, id);
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
                return new CommandHelper(() => MarkCompleted(SelectedTask));
            }
        }

        private void MarkCompleted(TaskItem task)
        {
            try
            {
                if (task != null && task.Completed == null)
                {
                    Workspace.API.MarkCompleted(task, DateTime.Today);

                    TasksChanged();
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
                int? pid = parent == null ? null : (int?)parent.TaskItemID;

                Workspace.API.DeleteTask(SelectedTask);

                TasksChanged();

                if (pid != null)
                    ExpandAndSelect(tasks, pid.Value);

                //Need to update list as well
                //TODO Duplicate: RefreshListsView();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        private void RefreshListsView()
        {
            Lists.Refresh();
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

        #region Misc

        public string HotKeyText
        {
            get
            {
                var hk = Hotkey.GetDefaultShowWindowHotkey();
                if (hk == null)
                    return "No Hotkey Set";
                else
                    return "CTRL + " + hk.DisplayName;
            }
        }

        public ICommand SetHotkeyCommand
        {
            get
            {
                return new CommandHelper(ShowHotkeySettings);
            }
        }

        private void ShowHotkeySettings()
        {
            var window = new HotKeySettingsWindow(ShowWindow);
            window.ShowDialog();

            if (window.HotKeyChanged)
                RaisePropertyChanged("HotkeyText");
        }

        #region Notify Icon

        public ICommand QuitCommand
        {
            get
            {
                return new CommandHelper(() => Close(false));
            }
        }

        public ICommand MinimizeCommand
        {
            get
            {
                return new CommandHelper(() => Close());
            }
        }

        public void Close(bool minimize = true)
        {
            if (minimize)
            {
                window.WindowState = WindowState.Minimized;
                window.Hide();
            }
            else
            {
                if (onQuit != null)
                    onQuit.Invoke();

                TasksUpdateTimer.StopTimer();
                Environment.Exit(0);
            }
        }

        public ICommand ShowCommand
        {
            get
            {
                return new CommandHelper(ShowWindow);
            }
        }

        private void ShowWindow()
        {
            if (window.WindowState == WindowState.Minimized)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
            }
            else //Just in background
                window.Activate();
        }

        #endregion

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

        public ICommand HistoryViewerCommand
        {
            get
            {
                return new CommandHelper(ShowHistoryViewerWindow);
            }
        }

        private void ShowHistoryViewerWindow()
        {
            var window = new HistoryViewerWindow();
            window.ShowDialog();

        }

        public ICommand CleanerCommand
        {
            get { return new CommandHelper(ShowCleaner); }
        }

        private void ShowCleaner()
        {
            var window = new CleanerWindow();
            window.ShowDialog();

            if (window.Cleaned)
                TasksChanged();
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

            if (dlg.NeedsRefresh)
            {
                ReloadLists();
                TasksChanged();
            }
        }

        #endregion


        #region Stats
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

        #endregion

        #region QuickList

        private const string Repeating = "Repeating";
        private const string ComingDue = "Coming Due";
        private const string Completed = "Completed";
        public const string Aging = "Aging";

        private static readonly List<string> quickLists = new List<string>()
        {
            ComingDue,
            Completed,
            Repeating,
            Aging
        };

        public List<string> QuickLists
        {
            get { return quickLists; }
        }

        private string selectedQuickListType;
        public string SelectedQuickListType
        {
            get { return selectedQuickListType; }
            set
            {
                if (value != selectedQuickListType)
                {
                    selectedQuickListType = value;
                    RaisePropertyChanged();

                    UpdateQuickList();
                }
            }
        }

        public ICollectionView QuickListView
        {
            get
            {
                return quickListSource.View;
            }
        }

        private void UpdateQuickList()
        {
            quickList.Clear();

            IEnumerable<TaskItem> query = null;

            //The ViewOrder is set by the order returned
            switch (SelectedQuickListType)
            {
                case ComingDue:
                    query = from i in Workspace.Instance.Tasks
                            where i.DueDate != null
                            && i.Completed == null
                            orderby i.DueDate ascending, i.Priority descending, i.Name ascending
                            select i;
                    break;
                case Completed:
                    query = from i in Workspace.Instance.Tasks
                            where i.Completed != null
                            orderby i.Completed descending, i.Priority descending, i.Name ascending
                            select i;
                    break;
                case Repeating:
                    query = from i in Workspace.Instance.Tasks
                            where i.Frequency != TaskFrequency.No
                            orderby (int)i.Frequency ascending, i.Priority descending, i.Name ascending
                            select i;
                    break;
                case Aging:
                    query = from i in Workspace.Instance.Tasks
                            where i.Completed == null
                            orderby i.Updated ascending, i.Name ascending
                            select i;
                    break;
                default:
                    break;
            }

            if (query != null)
            {
                foreach (var i in query.ToList())
                    quickList.Add(new TaskItemViewModel(i));
            }

            QuickListView.Refresh();

        }


        private TaskItemViewModel selectedQuickListItem;
        public TaskItemViewModel SelectedQuickListItem
        {
            get { return selectedQuickListItem; }
            set
            {
                selectedQuickListItem = value;
                RaisePropertyChanged();
            }
        }

        public ICommand EditQuickListItemCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    TaskItem item = GetSelectedQuickListTaskItem();
                    EditTask(item);
                });
            }
        }

        private TaskItem GetSelectedQuickListTaskItem()
        {
            return SelectedQuickListItem == null ? null : SelectedQuickListItem.Data;
        }

        public ICommand MarkQuickListItemCompletedCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    TaskItem item = GetSelectedQuickListTaskItem();
                    MarkCompleted(item);
                });
            }
        }

        public ICommand SelectInTasksCommand
        {
            get { return new CommandHelper(SelectInTask); }
        }

        private void SelectInTask()
        {
            if (SelectedQuickListItem == null)
                return;

            var item = SelectedQuickListItem.Data;
            Thread th = new Thread(() =>
            {
                DoSelect(item.ListID, item.TaskItemID);
            });

            th.Start();
        }

        private void DoSelect(int listId, int taskId)
        {
            var disp = App.Current.Dispatcher;

            var list = lists.FirstOrDefault(x => x.Data.TaskListID == listId);
            if (list == null)
                return;

            disp.Invoke(() => SelectedList = list);

            int tries = 0;
            while (tries < 10)
            {
                Thread.Sleep(100);

                var task = tasks.FirstOrDefault();

                if (task != null && task.Data.ListID == listId)
                {
                    disp.Invoke(() =>
                        {
                            ExpandAndSelect(tasks, taskId);
                            //task.Selected = true;
                        }
                    );
                    break;
                }

                tries++;
            }
        }
        #endregion
    }
}
