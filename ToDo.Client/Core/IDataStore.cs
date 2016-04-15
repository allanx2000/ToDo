using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core.Lists;

namespace ToDo.Client.Core
{
    interface IDataStore
    {
        int InsertList(TaskList list);
        void EditList(TaskList list);
        void DeleteList(int id);

        int InsertTask(Task task);
        int EditTask(Task task);
        int DeleteTask(Task task);


    }
}
