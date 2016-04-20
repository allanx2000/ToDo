using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ToDo.Client.ViewModels;

namespace ToDo.Client
{
    /// <summary>
    /// Interaction logic for ParentSelectWindow.xaml
    /// </summary>
    public partial class ParentSelectWindow : Window
    {
        private readonly ParentSelectWindowViewModel vm;

        public ParentSelectWindow(int listId, int? currentTaskId)
        {
            InitializeComponent();

            vm = new ParentSelectWindowViewModel(listId, currentTaskId, this);
            this.DataContext = vm;
        }

        public bool Cancelled
        {
            get
            {
                return vm.Cancelled;
            }
        }

        public TaskItemViewModel SelectedItem { get
            {
                return vm.SelectedItem;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            vm.SelectedItem = (TaskItemViewModel) e.NewValue;
        }
    }
}
