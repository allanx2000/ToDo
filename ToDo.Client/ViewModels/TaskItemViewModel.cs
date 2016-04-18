using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ToDo.Client.Core.Tasks;
using ToDo.Client.Properties;

namespace ToDo.Client.ViewModels
{
    public class TaskItemViewModel : ViewModel
    {
        private static Style Complete, Incomplete;

        TaskItemViewModel()
        {
            var bold = FontWeights.Bold;
            var current = Application.Current.FindResource(typeof(Label)) as Style;
            
            Complete = new Style(typeof(TextBlock));
            //Complete.BasedOn = current;
            Complete.Setters.Add(new Setter(TextBlock.FontWeightProperty, bold));
            Complete.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, TextDecorations.Strikethrough));
            Complete.Setters.Add(new Setter(TextBlock.ForegroundProperty, Colors.Gray));

            Incomplete = new Style(typeof(TextBlock));
            //Incomplete.BasedOn = current;
            Incomplete.Setters.Add(new Setter(TextBlock.FontWeightProperty, bold));
        }

        public TaskItemViewModel(TaskItem item)
        {
            data = item;
        }

        private TaskItem data;
        public TaskItem Data
        {
            get { return data; }
        }

        public string Name
        {
            get
            {
                return Data.Title;
            }
        }

        public int Order
        {
            get { return data.Order; }
        }

        public Style TextStyle
        {
            get
            {
                return data.Completed == null ? Incomplete : Complete;
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                isExpanded = value;
                RaisePropertyChanged();
            }
        }
    }
}
