using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ToDo.Client.ViewModels
{
    class LoadWindowViewModel : ViewModel
    {
        private string dbPath;

        public string DatabasePath
        {
            get { return dbPath; }
            set
            {
                dbPath = value;
                RaisePropertyChanged();
            }
        }

        public ICommand BrowseCommand
        {
            get { return new CommandHelper(BrowseForPath); }
        }

        private void BrowseForPath()
        {
            var fd = Innouvous.Utils.DialogsUtility.CreateFolderBrowser();
            fd.ShowDialog();

            if (!String.IsNullOrEmpty(fd.SelectedPath))
                DatabasePath = fd.SelectedPath;
        }
    }
}
