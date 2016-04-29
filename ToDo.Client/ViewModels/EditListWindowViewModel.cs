using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ToDo.Client.Core.Lists;

namespace ToDo.Client.ViewModels
{
    class EditListWindowViewModel : ViewModel
    {
        private Window window;
        private TaskList existing;

        public bool Cancelled { get; private set; }

        public EditListWindowViewModel(Window window, TaskList existing = null)
        {
            this.window = window;

            Cancelled = true;

            if (existing != null)
                LoadTaskList(existing);
        }

        private void LoadTaskList(TaskList list)
        {
            existing = list;
            Name = existing.Title;
            Description = existing.Description;
            SelectedListType = ConvertType(existing.Type);

            RaisePropertyChanged("Title");
            RaisePropertyChanged("CanEditType");
        }

        private bool HasExisting
        {
            get
            {
                return existing != null;
            }
        }

        private string ConvertType(ListType type)
        {
            switch (type)
            {
                case ListType.ToDo:
                    return ToDo;
                case ListType.Project:
                    return Project;
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }

        private ListType ConvertType(string type)
        {
            switch (type)
            {
                case ToDo:
                    return ListType.ToDo;
                case Project:
                    return ListType.Project;
                default:
                    throw new NotSupportedException(type);
            }
        }


        #region Properties and Commands

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                RaisePropertyChanged();
            }
        }

        public bool CanEditType
        {
            get
            {
                return HasExisting ? false : true;
            }
        }

        public string Title
        {
            get
            {
                return (!HasExisting ? "Add" : "Edit") + " List";
            }
        }

        private const string Project = "Project";
        private const string ToDo = "To Do List";

        private readonly List<string> listTypes = new List<string>()
        {
            Project,
            ToDo
        };

        private string selectedListType;

        public string SelectedListType
        {
            get
            {
                return selectedListType;
            }
            set
            {
                selectedListType = value;
                RaisePropertyChanged();
            }
        }

        public List<string> ListTypes
        {
            get
            {
                return listTypes;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new CommandHelper(() => window.Close());
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new CommandHelper(Save);
            }
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrEmpty(Name))
                    throw new Exception("Name cannot be empty");

                var duplicate = (from l in Workspace.Instance.Lists
                            where l.Title == Name select l).FirstOrDefault();

                if (duplicate != null)
                {
                    if (existing != null && duplicate.TaskListID == existing.TaskListID)
                    {
                        //OK
                    }
                    else
                    {
                        throw new Exception("A list by the same name already exists");
                    }
                }

                DateTime now = DateTime.Now;

                if (existing == null)
                {
                    
                    TaskList list = new TaskList()
                    {
                        Title = Name,
                        Description = Description,
                        Type = ConvertType(SelectedListType),
                        Created = now,
                        LastUpdated = now
                    };

                    Workspace.Instance.Lists.Add(list);
                }
                else
                {
                    existing.Title = Name;
                    existing.Description = Description;
                    existing.LastUpdated = now;
                }

                Workspace.Instance.SaveChanges();

                Cancelled = false;
                window.Close();
            }
            catch (Exception e)
            {
                Workspace.Instance.RejectChanges();
                MessageBoxFactory.ShowError(e);
            }
        }
        
        #endregion
    }
}
