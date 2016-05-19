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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToDo.Client.ViewModels;

namespace ToDo.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        private readonly LoadWindowViewModel vm;
        public LoadWindow()
        {
            InitializeComponent();

            vm =  new LoadWindowViewModel(this, Spinner);
            this.DataContext = vm;

            //Spinner.IsAdornerVisible = true;
        }

        /// <summary>
        /// Fully exits app if quit without loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            if (Workspace.Instance == null)
            {
                Environment.Exit(0);
            }
        }
    }
}
