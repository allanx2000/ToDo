using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ToDo.Client.ViewModels
{
    public class CleanerViewModel : ViewModel
    {
        private readonly Workspace DB = Workspace.Instance;

        private Action<bool> onClose; //shouldReload
        public CleanerViewModel(Action<bool> onClose)
        {
            this.onClose = onClose;
            tasks = new ObservableCollection<TaskItemViewModel>();

            SelectedDate = StartDate;
        }

        public DateTime StartDate
        {
            get {
                if (DB.Tasks.Count() == 0)
                    return DateTime.Today;
                else
                {
                    return (from t in DB.Tasks
                            where t.Completed != null 
                                && t.Frequency == Core.Tasks.TaskFrequency.No
                            select t).Min(x => x.Completed).Value.AddDays(1);
                }
            }
        
        }

        private DateTime? selectedDate;
        public DateTime? SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;

                LoadTasks(selectedDate);

                RaisePropertyChanged();
            }
        }

        private void LoadTasks(DateTime? date)
        {
            if (date == null)
                return;

            var query = from t in DB.Tasks
                        where t.Completed != null 
                            && t.Completed < date.Value && t.ParentID == null
                            && t.Frequency == Core.Tasks.TaskFrequency.No
                        orderby t.Completed descending
                        select t;

            tasks.Clear();

            DateTime min = DateTime.Today;
            foreach (var t in query.ToList())
            {
                if (t.Completed < min)
                    min = t.Completed.Value;

                tasks.Add(new TaskItemViewModel(t));
            }
            
        }

        private ObservableCollection<TaskItemViewModel> tasks;
        public ICollection<TaskItemViewModel> Tasks
        {
            get { return tasks; }
        }

        public int Count
        {
            get { return tasks.Count; }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new CommandHelper(DeleteTasks);
            }
        }

        private bool shouldReload = false;

        private void DeleteTasks()
        {
            foreach (TaskItemViewModel t in tasks)
            {
                DB.Tasks.Remove(t.Data);
            }

            DB.SaveChanges();

            RaisePropertyChanged("StartDate");
            SelectedDate = StartDate;

            shouldReload = true;
        }

        public ICommand CloseCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    if (onClose != null)
                        onClose.Invoke(shouldReload);
                });
            }
        }
    }
}
