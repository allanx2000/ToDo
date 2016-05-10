using Innouvous.Utils.MVVM;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ToDo.Client.Core.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ToDo.Client.ViewModels
{

    public class LogsViewerViewModel : ViewModel
    {
        private System.Windows.Controls.Calendar calendar;
        private CollectionViewSource tasksView;
        private ObservableCollection<TaskItemViewModel> tasks = new ObservableCollection<TaskItemViewModel>();
        private Window window;
        
        public DateStyleConverter Converter { get; private set; }
        
        public LogsViewerViewModel(Window window, System.Windows.Controls.Calendar calendar) //TODO: Should use callback to UI instead?
        {
            this.window = window;
            this.calendar = calendar;

            Converter = new DateStyleConverter();

            tasksView = new CollectionViewSource();
            tasksView.Source = tasks;
            LoadTasksList();
        }

        private void LoadTasksList()
        {
            var list = (from t in Workspace.Instance.TasksLog select t.Task).Distinct();

            tasks.Clear();
            foreach (var t in list)
                tasks.Add(new TaskItemViewModel(t));

        }

        public ICollectionView Tasks
        {
            get { return tasksView.View; }
        }

        private TaskItemViewModel selectedTask;
        
        public TaskItemViewModel SelectedTask
        {
            get { return selectedTask; }
            set
            {
                selectedTask = value;
                UpdateCalendar();

                RaisePropertyChanged();
            }
        }

        private void UpdateCalendar()
        {
            if (SelectedTask != null)
            {
                int taskId = SelectedTask.Data.TaskItemID;

                var logs = Workspace.Instance.TasksLog
                    .Where(x => x.TaskID == taskId && x.Completed)
                    .OrderBy(x => x.Date);

                calendar.DisplayDateEnd = calendar.DisplayDate = DateTime.Today;

                DateStyleConverter.Dates = (from d in logs select d.Date).ToList();

                var tmp = calendar.CalendarDayButtonStyle;
                calendar.CalendarDayButtonStyle = null;
                calendar.CalendarDayButtonStyle = tmp;
            }
        }

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

                UpdateDetails();

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
            }
        }

        public bool NoSelected { get { return !yesSelected; } }

        private void UpdateDetails()
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

        public ICommand CloseCommand
        {
            get
            {
                return new CommandHelper(() => window.Close());
            }
        }
    }
}
