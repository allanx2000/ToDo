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
            public List<TaskItem> Tasks { get; set; }
        }

        public static void Export(string path)
        {
            Bundle bundle = new Bundle()
            {
                Lists = Workspace.Instance.Lists.ToList(),
                Log = Workspace.Instance.TasksLog.ToList(),
                Tasks = Workspace.Instance.Tasks.ToList()
            };

            XmlSerializer xser = new XmlSerializer(typeof(Bundle));
            StreamWriter sw = new StreamWriter(@"c:\dev\todo.xml");
            xser.Serialize(sw, bundle);
            sw.Close();
            //Manual Serialization, Use JSON
        }

        internal static void Import(string importPath)
        {
            throw new NotImplementedException();
        }
    }
}
