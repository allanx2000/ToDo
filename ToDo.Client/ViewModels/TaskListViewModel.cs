using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ToDo.Client.Core.Lists;

namespace ToDo.Client.ViewModels
{
    public class TaskListViewModel : ViewModel
    {
        private static SolidColorBrush projectColor = new SolidColorBrush(Colors.DarkRed);
        private static SolidColorBrush todoColor = new SolidColorBrush(Colors.Black);
        
        private TaskList list;

        public TaskListViewModel(TaskList list)
        {
            this.list = list;
        }

        public void Update()
        {
            RaisePropertyChanged("Remaining");
            RaisePropertyChanged("Completed");
        }

        #region Properties

        public TaskList Data
        {
            get
            {
                return list;
            }
        }
        
        public string Name
        {
            get
            {
                return Data.Name;
            }
        }

        public int Completed
        {
            get
            {
                return Data.TaskItems == null ? 0 : (from t in Data.TaskItems where t.Completed != null select t).Count();
            }
        }

        public int Remaining
        {
            get
            {
                return Data.TaskItems == null? 0 : (from t in Data.TaskItems where t.Completed == null select t).Count();
            }
        }

        public ListType Type { get { return Data.Type; } }

        public SolidColorBrush NameColor
        {
            get
            {
                switch (list.Type)
                {
                    case ListType.Project:
                        return projectColor;
                    case ListType.ToDo:
                    default:
                        return todoColor;
                }
            }
        }

        #endregion
    }
}
