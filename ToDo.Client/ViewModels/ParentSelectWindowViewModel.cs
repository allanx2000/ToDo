using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ToDo.Client.ViewModels
{
    class ParentSelectWindowViewModel : ViewModel
    {
        private ObservableCollection<TaskItemViewModel> tasks = new ObservableCollection<TaskItemViewModel>();
        private CollectionViewSource viewsource;
        private Window window;
        private int? currentTaskId; //Prevent looping
        
        public ParentSelectWindowViewModel(int listId, int? currentTaskId, Window window)
        {
            this.window = window;
            this.currentTaskId = currentTaskId;

            Cancelled = true;

            viewsource = new CollectionViewSource();
            viewsource.Source = tasks;
            viewsource.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));

            Workspace.API.LoadList(listId, tasks, currentTaskId);
            
        }

        public bool Cancelled { get; private set; }

        public TaskItemViewModel SelectedItem { get; set; }

        public ICollectionView Tasks
        {
            get
            {
                return viewsource.View;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    Cancelled = true;
                    window.Close();
                });
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    SelectedItem = null;
                    Cancelled = false;
                    window.Close();    
                });
            }
        }

        public ICommand OKCommand
        {
            get
            {
                return new CommandHelper(Select);
            }
        }

        private void Select()
        {
            try
            {
                if (SelectedItem == null)
                    throw new Exception("No parent selected.");
                else if (currentTaskId != null && SelectedItem.Data.TaskItemID == currentTaskId.Value)
                    throw new Exception("Parent task cannot be itself.");
                
                this.Cancelled = false;

                window.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }
        
    }
}
