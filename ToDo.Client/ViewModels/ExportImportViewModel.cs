using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using EXP = ToDo.Client.Export.Exporter;

namespace ToDo.Client.ViewModels
{
    public class ExportImportViewModel : ViewModel
    {
        private Window window;
        
        public ExportImportViewModel(Window window)
        {
            this.window = window;

            NeedsReload = false;
        }

        //Needs to reload lists and tasks
        public bool NeedsReload { get; private set; }

        #region Export
        private string exportPath;
        public string ExportPath {
            get
            {
                return exportPath;
            }
            set
            {
                exportPath = value;
                RaisePropertyChanged();
            }
        }

        public ICommand BrowseForExportCommand
        {
            get
            {
                return new CommandHelper(BrowseForExport);
            }
        }

        private void BrowseForExport()
        {
            var sfd = DialogsUtility.CreateSaveFileDialog("Export");
            DialogsUtility.AddExtension(sfd, "XML File", "*.xml");

            sfd.ShowDialog();

            ExportPath = sfd.FileName;
        }

        public ICommand ExportCommand
        {
            get
            {
                return new CommandHelper(Export);
            }
        }

        public void Export()
        {
            try
            {
                if (string.IsNullOrEmpty(ExportPath))
                    throw new Exception("Path cannot be empty.");

                EXP.Export(ExportPath);
                
                MessageBoxFactory.ShowInfo("Database exported to: " + ExportPath, "Exported");                
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        #endregion

        #region Import
        private string importPath;
        public string ImportPath
        {
            get
            {
                return importPath;
            }
            set
            {
                importPath = value;
                RaisePropertyChanged();
            }
        }

        public ICommand BrowseForImportCommand
        {
            get
            {
                return new CommandHelper(BrowseForImport);
            }
        }

        private void BrowseForImport()
        {
            var ofd = DialogsUtility.CreateOpenFileDialog("Import");
            DialogsUtility.AddExtension(ofd, "XML File", "*.xml");

            ofd.ShowDialog();

            ImportPath = ofd.FileName;
        }

        public ICommand ImportCommand
        {
            get
            {
                return new CommandHelper(Import);
            }
        }

        public void Import()
        {
            try
            {
                if (string.IsNullOrEmpty(importPath) || !File.Exists(ImportPath))
                    throw new Exception("File not found.");

                EXP.Import(importPath);
                
                MessageBoxFactory.ShowInfo("Database has been imported.", "Imported");

                NeedsReload = true;
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        #endregion

        public ICommand ClearDatabaseCommand
        {
            get
            {
                return new CommandHelper(ClearDatabase);
            }
        }

        private void ClearDatabase()
        {
            if (MessageBoxFactory.ShowConfirmAsBool("Are you sure you want to clear the database?", "Confirm Clear Database", MessageBoxImage.Exclamation))
            {
                var DB = Workspace.Instance;

                DB.Comments.RemoveRange(DB.Comments);
                DB.Lists.RemoveRange(DB.Lists);
                DB.Tasks.RemoveRange(DB.Tasks);
                DB.TasksLog.RemoveRange(DB.TasksLog);

                NeedsReload = true;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new CommandHelper(() => window.Close());
            }
        }
    }
}