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
    /// Interaction logic for CleanerWindow.xaml
    /// </summary>
    public partial class CleanerWindow : Window
    {
        public readonly CleanerViewModel vm;

        public CleanerWindow()
        {
            InitializeComponent();

            vm = new CleanerViewModel((cleaned) =>
                {
                    Cleaned = cleaned;
                    this.Close();
                });

            this.DataContext = vm;
        }

        public bool Cleaned { get; internal set; }
    }
}
