using Innouvous.Utils.MVVM;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ToDo.Client.Core.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Innouvous.Utils;

namespace ToDo.Client.ViewModels
{

    public class HistoryViewerViewModel : ViewModel
    {
        private System.Windows.Controls.Calendar calendar;
        private CollectionViewSource tasksView;
        private ObservableCollection<TaskItemViewModel> tasks = new ObservableCollection<TaskItemViewModel>();
        private Window window;
        
        public HistoryViewerViewModel(Window window, System.Windows.Controls.Calendar calendar) //TODO: Should use callback to UI instead?
        {
            this.window = window;
            this.calendar = calendar;

            DateStyleConverter.Logs = null;

            calendar.DisplayDateStart = GetStartDate();
            calendar.DisplayDateEnd = DateTime.Today.AddDays(-1);
            calendar.DisplayDate = calendar.DisplayDateEnd.Value;

            SelectedDate = calendar.DisplayDateEnd;
            
            tasksView = new CollectionViewSource();
            tasksView.Source = tasks;
            LoadTasksList();
        }

        private static DateTime GetStartDate()
        {
            DateTime dt = DateTime.Today.AddMonths(-3);
            var first = Workspace.Instance.TasksLog.OrderBy(x => x.Date).FirstOrDefault();
            if (first != null)
                dt = first.Date;

            return dt;
        }

        private void LoadTasksList()
        {
            var list = (from t in Workspace.Instance.TasksLog select t.Task).Distinct().OrderBy(x => x.Name);

            tasks.Clear();
            foreach (var t in list)
                tasks.Add(new TaskItemViewModel(t));

        }

        public ICollectionView Tasks
        {
            get { return tasksView.View; }
        }

        public bool TaskSelected { get { return selectedTask != null; } }

        private TaskItemViewModel selectedTask;
        public TaskItemViewModel SelectedTask
        {
            get { return selectedTask; }
            set
            {
                selectedTask = value;
                UpdateCalendar();
                UpdateRadioBoxes();

                RaisePropertyChanged();
                RaisePropertyChanged("TaskSelected");
            }
        }

        /// <summary>
        /// Updates the Calendar highlighting the log dates
        /// </summary>
        private void UpdateCalendar()
        {
            if (SelectedTask != null)
            {
                int taskId = SelectedTask.Data.TaskItemID;

                var logDates = Workspace.Instance.TasksLog
                    .Where(x => x.TaskID == taskId) //&& x.Completed
                    .OrderBy(x => x.Date);

                DateStyleConverter.Logs = logDates.ToList();
                
                //Redraw Calendar
                var tmp = calendar.CalendarDayButtonStyle;
                calendar.CalendarDayButtonStyle = null;
                calendar.CalendarDayButtonStyle = tmp;
            }
        }

        #region Deprecated
        //TODO: Remove
        private DateTime? selectedDate;           
        public DateTime? SelectedDate
        {
            get
            {
                return selectedDate;
            }
            set
            {
                selectedDate = value;
                RaisePropertyChanged();
                UpdateRadioBoxes();

                RaisePropertyChanged();
            }
        }

        private bool yesSelected;
        public bool YesSelected {
            get { return yesSelected; }
            set
            {
                yesSelected = value;
                
                RaisePropertyChanged();
                RaisePropertyChanged("NoSelected");
                
                SetCompleted(value);
            }
        }

        private void SetCompleted(bool completed)
        {
            //Deprecated, no changing
            /*
            if (SelectedTask != null && SelectedDate != null)
            {
                Workspace.API.LogCompleted(SelectedDate.Value, SelectedTask.Data.TaskItemID, completed);
                UpdateCalendar();
            }
            */
        }

        public bool NoSelected { get { return !yesSelected; } }

        private void UpdateRadioBoxes()
        {
            if (SelectedDate != null && SelectedTask != null)
            {
                var log = GetLog(SelectedTask.Data, SelectedDate.Value);

                if (log != null)
                {
                    YesSelected = log.Completed;
                    return;
                }
            }

            YesSelected = false;
        }

        private TaskLog GetLog(TaskItem task, DateTime date)
        {
            var log = Workspace.Instance.TasksLog
                                .Where(x => x.TaskID == task.TaskItemID
                                    && x.Date == date).FirstOrDefault();

            return log;
        }
        #endregion

        public ICommand CloseCommand
        {
            get
            {
                return new CommandHelper(() => window.Close());
            }
        }

        public ICommand ClearLogsCommand
        {
            get
            {
                return new CommandHelper(ClearLogs);
            }
        }

        private void ClearLogs()
        {
            try
            {
                if (selectedTask == null)
                    return;
                else if (!MessageBoxFactory.ShowConfirmAsBool("Delete all log entries?", "Confirm Delete"))
                    return;

                foreach (var t in Workspace.Instance.TasksLog.Where(x => x.TaskID == SelectedTask.Data.TaskItemID))
                    Workspace.Instance.TasksLog.Remove(t);

                Workspace.Instance.SaveChanges();
                UpdateCalendar();
            }
            catch (Exception e)
            {

            }
        }
    }
}
