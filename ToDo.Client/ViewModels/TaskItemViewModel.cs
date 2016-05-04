using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ToDo.Client.Core.Tasks;
using ToDo.Client.Properties;

namespace ToDo.Client.ViewModels
{
    public class TaskItemViewModel : ViewModel
    {
        private static Style Complete, Incomplete;

        static TaskItemViewModel()
        {
            //Create Styles
            var bold = FontWeights.Bold;
            //var current = Application.Current.FindResource(typeof(Label)) as Style;
            
            Complete = new Style(typeof(TextBlock));
            Complete.Setters.Add(new Setter(TextBlock.FontWeightProperty, bold));
            Complete.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, TextDecorations.Strikethrough));
            Complete.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.LightGray)));

            Incomplete = new Style(typeof(TextBlock));
            Incomplete.Setters.Add(new Setter(TextBlock.FontWeightProperty, bold));
        }

        private static readonly SolidColorBrush Green = new SolidColorBrush(Colors.DarkGreen);
        private static readonly SolidColorBrush Orange = new SolidColorBrush(Colors.Orange);
        private static readonly SolidColorBrush Red = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush Black = new SolidColorBrush(Colors.Black);
        
        public TaskItemViewModel(TaskItem item)
        {
            data = item;

            //Create children view models
            if (data.Children != null)
            {
                children = new List<TaskItemViewModel>();
                foreach (var c in data.Children)
                {
                    var vm = new TaskItemViewModel(c);
                    vm.Parent = this;

                    children.Add(vm);
                }
            }

            childrenViewSource = new CollectionViewSource();
            childrenViewSource.Source = children;
            childrenViewSource.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
        }

        #region Properties

        public CollectionViewSource childrenViewSource;
        public ICollectionView ChildrenView
        {
            get
            {
                return childrenViewSource.View;
            }
        }


        private List<TaskItemViewModel> children;
        public ICollection<TaskItemViewModel> Children
        {
            get
            {
                return children;
            }
        }

        private TaskItem data;
        public TaskItem Data
        {
            get { return data; }
        }

        public TaskItemViewModel Parent { get; set; }

        public bool IsComplete
        {
            get
            {
                return Data.Completed != null;
            }
        }

        public Style TextStyle
        {
            get
            {
                return data.Completed == null ? Incomplete : Complete;
            }
        }

        public string DisplayName
        {
            get
            {
                return Data.Order + ". " + data.Title;
            }
        }

        public string Name
        {
            get
            {
                return Data.Title;
            }
        }

        public string DescriptionText
        {
            get
            {
                return string.IsNullOrEmpty(Data.Description) ? "{No Description}" : Data.Description;
            }
        }

        public int Priority
        {
            get { return Data.Priority; }
        }

        public int Order
        {
            get { return data.Order; }
        }

        public string FrequencyText
        {
            get
            {
                return Data.Frequency == null ? "No" : Data.Frequency.ToString();
            }
        }

        public int CommentsCount
        {
            get { return Data.Comments == null ? 0 : Data.Comments.Count; }
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

        #region Completed/DueDate

        public string CompletedDate
        {
            get
            {
                return data.Completed == null ? "No" : data.Completed.Value.ToShortDateString();
            }
        }
        public string DueDate
        {
            get
            {
                if (data.DueDate == null)
                    return "Not Set";
                else
                    return data.DueDate.Value.ToShortDateString();
            }
        }

        public SolidColorBrush DueDateColor
        {
            get
            {
                SolidColorBrush color = Black;

                var date = data.DueDate;
                var today = DateTime.Today;

                if (data.Completed == null && date != null)
                {
                    var diff = date.Value - today;
                    int days = diff.Days;

                    if (days <= 0)
                        color = Red;
                    else if (days < 5)
                        color = Orange;
                    else
                        color = Green;
                }

                return color;
            }
        }

        #endregion

        
        #endregion
    }
}
