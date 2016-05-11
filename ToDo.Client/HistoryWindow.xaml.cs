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
    public partial class HistoryViewerWindow : Window
    {
        private readonly HistoryViewerViewModel vm;
        public HistoryViewerWindow()
        {
            InitializeComponent();

            vm = new HistoryViewerViewModel(this, Calendar);
            this.DataContext = vm;


        }
    }
}
