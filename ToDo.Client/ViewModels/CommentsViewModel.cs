using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core;

namespace ToDo.Client.ViewModels
{
    public class CommentsViewModel : ViewModel
    {
        private Comment data;

        public CommentsViewModel(Comment comment)
        {
            data = comment;
        }

        public string Text
        {
            get { return data.Text; }
        }

        public DateTime Created
        { get { return data.Created; } }
        
    }
}
