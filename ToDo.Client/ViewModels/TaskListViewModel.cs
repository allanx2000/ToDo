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
        private TaskList list;

        public TaskList Data
        {
            get
            {
                return list;
            }
        }

        private static SolidColorBrush projectColor = new SolidColorBrush(Colors.DarkRed);
        private static SolidColorBrush todoColor = new SolidColorBrush(Colors.Black);

        public string Name
        {
            get
            {
                return Data.Title;
            }
        }

        public SolidColorBrush TitleColor
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

        public TaskListViewModel(TaskList list)
        {
            this.list = list;
        }

    }
}
