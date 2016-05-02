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
using ToDo.Client.Core.Lists;
using ToDo.Client.Core.Tasks;
using ToDo.Client.ViewModels;

namespace ToDo.Client
{
    /// <summary>
    /// Interaction logic for EditTaskWindow.xaml
    /// </summary>
    public partial class EditTaskWindow : Window
    {
        private readonly EditTaskWindowViewModel vm;

        /// <summary>
        /// Add Task/Subtask
        /// </summary>
        /// <param name="list"></param>
        /// <param name="parent"></param>
        public EditTaskWindow(TaskList list, TaskItem parent = null)
        {
            InitializeComponent();
            vm = new EditTaskWindowViewModel(this, list);
            this.DataContext = vm;

            vm.SetParent(parent);
        }

        /// <summary>
        /// Edit Task
        /// </summary>
        /// <param name="existing"></param>
        public EditTaskWindow(TaskItem existing)
        {
            InitializeComponent();

            vm = new EditTaskWindowViewModel(this, existing);
            this.DataContext = vm;
            
        }

        public bool Cancelled
        {
            get
            {
                return vm.Cancelled;
            }
        }
    }
}
