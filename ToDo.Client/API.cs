using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core.Lists;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client
{
    public partial class Workspace : DbContext
    {
        public static class API
        {
            public static Workspace DB
            {
                get
                {
                    return Workspace.Instance;
                }
            }

            public static IEnumerable<TaskList> GetLists()
            {
                return DB.Lists.Include(x => x.TaskItems);
            }
        }
    }
}
