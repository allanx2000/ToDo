using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ToDo.Client.Core.Lists;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client.Export
{
    public static class Exporter
    {
        [Serializable]
        public class Bundle
        {
            public List<TaskList> Lists { get; set; }
            public List<TaskLog> Log { get; set; }
        }

        public static void Export(string path)
        {
            //Manual Serialization, Use JSON
        }
    }
}
