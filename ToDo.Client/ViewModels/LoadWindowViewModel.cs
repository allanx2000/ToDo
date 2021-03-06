﻿using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ToDo.Client.Core.Lists;
using AdornedControl;

namespace ToDo.Client.ViewModels
{
    class LoadWindowViewModel : ViewModel
    {
        private readonly Properties.Settings Settings = Properties.Settings.Default;

        private string workspacePath;
        private Window window;

        private readonly Dispatcher GUI = App.Current.Dispatcher;

        public LoadWindowViewModel(Window window, AdornedControl.AdornedControl spinner)
        {
            this.window = window;
            this.spinner = spinner;
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
            var fd = DialogsUtility.CreateFolderBrowser();
            fd.ShowDialog();

            if (!String.IsNullOrEmpty(fd.SelectedPath))
                WorkspacePath = fd.SelectedPath;
        }

        public ICommand LoadCommand
        {
            get { return new CommandHelper(Load); }
        }

        private const string DatabaseFile = "db.sqlite";
        private AdornedControl.AdornedControl spinner;

        private string GetDbFilePath(string folder)
        {
            return Path.Combine(folder, DatabaseFile);
        }

        public void Load()
        {
            try
            {
                if (string.IsNullOrEmpty(WorkspacePath))
                    throw new Exception("The path cannot be empty.");

                string dbFile = GetDbFilePath(WorkspacePath);

                if (!Directory.Exists(WorkspacePath))
                {
                    Directory.CreateDirectory(WorkspacePath);
                }
                else
                    ValidateWorkspace(workspacePath);

                LoadWorkspaceAsync(dbFile);

            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);

            }
        }

        private void LoadWorkspaceAsync(string dbFile)
        {
            spinner.IsAdornerVisible = true;
            window.IsEnabled = false;

            Thread th = new Thread(() =>
            {
                try
                {
                    
                    Workspace.LoadWorkspace(workspacePath, dbFile);

                    Settings.LastPath = WorkspacePath;
                    Settings.Save();

                    GUI.Invoke(() =>
                    {
                        DashboardWindow dashboard = new DashboardWindow();
                        dashboard.Show();
                        window.Close();
                    });

                }
                catch (Exception e)
                {
                    GUI.Invoke(() => MessageBoxFactory.ShowError(e));
                }
                finally
                {
                    GUI.Invoke(() =>
                    {
                        window.IsEnabled = true;
                        spinner.IsAdornerVisible = false;
                    });
                }
            });

            th.Start();
        }

        private void OnLoadTimer(object state)
        {
            App.Current.Dispatcher.Invoke(() => window.Title += ".");
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
