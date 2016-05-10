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
    /// Interaction logic for LogsViewerWindow.xaml
    /// </summary>
    public partial class LogsViewerWindow : Window
    {
        private readonly LogsViewerViewModel vm;
        public LogsViewerWindow()
        {
            InitializeComponent();

            vm = new LogsViewerViewModel(this, Calendar);
            this.DataContext = vm;


        }
    }
}
