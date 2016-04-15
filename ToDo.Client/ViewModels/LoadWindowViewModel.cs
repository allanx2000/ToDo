using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ToDo.Client.ViewModels
{
    class LoadWindowViewModel : ViewModel
    {
        private string dbPath;
        private Window window;

        public LoadWindowViewModel(Window window)
        {
            this.window = window;
        }

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


        public ICommand LoadCommand
        {
            get { return new CommandHelper(Load); }
        }

        public void Load()
        {
            try
            {
                if (String.IsNullOrEmpty(DatabasePath))
                    throw new Exception("The path cannot be empty");
               
                if (Directory.Exists(DatabasePath)) //Attempt to load
                {

                }
                else //Create
                {
                    Directory.CreateDirectory(DatabasePath);
                }

                DashboardWindow db = new DashboardWindow(DatabasePath);
                db.Show();
                window.Close();
                
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);

            }
        }
    }
}
