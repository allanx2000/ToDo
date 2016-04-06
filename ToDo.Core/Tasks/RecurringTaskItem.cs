using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Core.Tasks
{
    class RecurringTaskItem : TaskItem
    {
        enum Frequency
        {
            Daily,
            Weekly,
            Monthly
        }
    }
}
