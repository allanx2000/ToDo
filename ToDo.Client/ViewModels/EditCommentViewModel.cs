using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ToDo.Client.Core;

namespace ToDo.Client.ViewModels
{
    public class EditCommentViewModel : ViewModel
    {
        private Comment existing;
        private Window window;


        private bool IsEdit
        {
            get { return existing != null; }
        }

        public EditCommentViewModel(Window window, Comment existing = null)
        {
            this.window = window;
            Cancelled = true;
            
            LoadExisting(existing);
        }

        private void LoadExisting(Comment existing)
        {
            if (existing == null)
                return;

            this.existing = existing;
            comment = existing.Text;

            RaisePropertyChanged("Title");
            RaisePropertyChanged("Comment");
            RaisePropertyChanged("DateVisibility");
            RaisePropertyChanged("CreatedDate");
            RaisePropertyChanged("SaveText");
        }

        public Comment Data { get; private set; }

        public bool Cancelled
        {
            get; private set;
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set
            {
                comment = value;
                RaisePropertyChanged();
            }
        }
        
        public Visibility DateVisibility
        {
            get
            {
                return IsEdit ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string CreatedDate
        {
            get
            {
                if (IsEdit)
                    return existing.Created.ToString("MMMM d, yyyy h:mm tt");
                else
                    return "";
            }
        }

        public string SaveText
        {
            get
            { return IsEdit ? "Edit" : "Add"; }
        }

        public string Title
        {
            get
            {
                return (IsEdit ? "Edit" : "Add") + " Comment";
            }
        }

        public ICommand CancelCommand
        {
            get { return new CommandHelper(Cancel); }
        }

        private void Cancel()
        {
            this.Cancelled = true;
            this.Data = null;

            window.Close();
        }

        public ICommand SaveCommand
        {
            get { return new CommandHelper(Save); }
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrEmpty(Comment))
                    throw new Exception("The comment cannot be empty.");

                Data = new Core.Comment()
                {
                    Text = Comment,
                };

                //Returns a List and then API checks if it has ID, if yes update, else add

                if (IsEdit)
                    Data.CommentID = existing.CommentID;
                else
                    Data.Created = DateTime.Now;

                Cancelled = false;
                window.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
            
        }
    }
}
