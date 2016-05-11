using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Client.ViewModels
{
    public static class SortDescriptions
    {

        public static readonly SortDescriptionCollection TaskItemsOrder = new SortDescriptionCollection();
        //public static readonly SortDescriptionCollection TaskItemsChildrenOrder = new SortDescriptionCollection();

        static SortDescriptions()
        {
            //TaskItemsChildrenOrder.Add(new SortDescription("IsComplete", ListSortDirection.Ascending));
            //TaskItemsChildrenOrder.Add(new SortDescription("Order", ListSortDirection.Ascending));

            TaskItemsOrder.Add(new SortDescription("IsComplete", ListSortDirection.Ascending));
            TaskItemsOrder.Add(new SortDescription("Order", ListSortDirection.Ascending));
        }


        public static void SetSortDescription(SortDescriptionCollection to, SortDescriptionCollection from)
        {
            to.Clear();

            foreach (var sd in from)
                to.Add(sd);
        }
    }
}
