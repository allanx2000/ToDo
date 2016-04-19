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
using ToDo.Client.Core.Lists;

namespace ToDo.Client.ViewModels
{
    class LoadWindowViewModel : ViewModel
    {
        private readonly Properties.Settings Settings = Properties.Settings.Default;

        private string workspacePath; 
        private Window window;

        public LoadWindowViewModel(Window window)
        {
            this.window = window;
            workspacePath = Settings.LastPath;
        }

        public string WorkspacePath
        {
            get { return workspacePath; }
            set
            {
                workspacePath = value;
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
                WorkspacePath = fd.SelectedPath;
        }

        public ICommand LoadCommand
        {
            get { return new CommandHelper(Load); }
        }

        private const string DatabaseFile = "db.sqlite";
        private string GetDbFilePath(string folder)
        {
            return Path.Combine(folder, DatabaseFile);
        }

        public void Load()
        {
            try
            {
                if (string.IsNullOrEmpty(WorkspacePath))
                    throw new Exception("The path cannot be empty");

                string dbFile = GetDbFilePath(WorkspacePath);

                if (!Directory.Exists(WorkspacePath))
                {
                    Directory.CreateDirectory(WorkspacePath);
                }
                else
                    ValidateWorkspace(workspacePath);

                Workspace.LoadWorkspace(workspacePath, dbFile);

                Settings.LastPath = WorkspacePath;
                Settings.Save();

                DashboardWindow dashboard = new DashboardWindow();
                dashboard.Show();
                window.Close();

            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);

            }
        }

        private void ValidateWorkspace(string workspacePath)
        {
            if (!File.Exists(GetDbFilePath(workspacePath)))
            {
                throw new Exception("The workspace is invalid.");
            }
        }
    }
}
