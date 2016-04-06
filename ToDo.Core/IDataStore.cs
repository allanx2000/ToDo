using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Core.Lists;

namespace ToDo.Core
{
    interface IDataStore
    {
        int InsertList(BaseList list);
        void EditList(BaseList list);
        void DeleteList(int id);

        int InsertTask(Task task);
        int EditTask(Task task);
        int DeleteTask(Task task);


    }
}
