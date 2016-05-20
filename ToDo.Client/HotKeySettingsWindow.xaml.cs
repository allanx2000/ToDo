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
    /// Interaction logic for SelectHotkeyWindow.xaml
    /// </summary>
    public partial class HotKeySettingsWindow : Window
    {
        private readonly HotKeySettingsViewModel vm;

        public HotKeySettingsWindow(Action showDashboardCallback)
        {
            InitializeComponent();

            vm = new HotKeySettingsViewModel(showDashboardCallback, 
                () => this.Close());

            this.DataContext = vm;
        }
    }
}
