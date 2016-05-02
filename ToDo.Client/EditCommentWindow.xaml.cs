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
using ToDo.Client.Core;
using ToDo.Client.ViewModels;

namespace ToDo.Client
{
    /// <summary>
    /// Interaction logic for EditCommentWindow.xaml
    /// </summary>
    public partial class EditCommentWindow : Window
    {
        private readonly EditCommentViewModel vm;

        public EditCommentWindow()
        {
            vm = new EditCommentViewModel(this, null);
            Initialize();
        }
        
        public EditCommentWindow(CommentViewModel existing)
        {
            vm = new EditCommentViewModel(this, existing.Data);
            Initialize();
        }
        
        private void Initialize()
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        public bool Cancelled
        {
            get { return vm.Cancelled; }
        }

        public Comment GetData()
        {
            return vm.Data;
        }
    }
}
