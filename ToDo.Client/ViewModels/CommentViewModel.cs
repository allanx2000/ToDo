using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core;
using System.Windows;

namespace ToDo.Client.ViewModels
{
    public class CommentViewModel : ViewModel
    {
        private Comment data;

        public CommentViewModel(Comment comment)
        {
            data = comment;
        }

        public Comment Data
        {
            get { return data; }
        }

        public string Text
        {
            get { return data.Text; }
        }

        public string Created
        {
            get
            {
                return Data.Created.ToString("MMMM d, yyyy h:mm tt");
            }
        }        
    }
}
