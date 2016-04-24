using System.Windows;
using ToDo.Client.ViewModels;

namespace ToDo.Client
{
    /// <summary>
    /// Interaction logic for ExportImportWindow.xaml
    /// </summary>
    public partial class ExportImportWindow : Window
    {
        private readonly ExportImportViewModel vm;

        public ExportImportWindow()
        {
            InitializeComponent();

            vm = new ExportImportViewModel(this);
            this.DataContext = vm;
        }
    }
}
