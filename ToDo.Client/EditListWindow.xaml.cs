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
using ToDo.Client.ViewModels;

namespace ToDo.Client
{
    /// <summary>
    /// Interaction logic for EditListWindow.xaml
    /// </summary>
    public partial class EditListWindow : Window
    {
        private readonly EditListWindowViewModel vm;

        public EditListWindow(TaskListViewModel existing) : this(existing.Data)
        {
        }

        public EditListWindow(TaskList existing = null)
        {
            InitializeComponent();

            vm = new EditListWindowViewModel(this, existing);
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
