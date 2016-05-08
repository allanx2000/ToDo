using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client
{
    static class TasksUpdateTimer
    {
        public delegate void TasksUpdatedEvent();

        public static event TasksUpdatedEvent OnTasksUpdated;

        private static Timer timer;

        private static int TickTime = 30 * 60 * 1000;

        public static void StartTimer()
        {
            if (timer != null)
                return;

            timer = new Timer(UpdateTasks, null, TickTime, TickTime);
        }

        public static void StopTimer()
        {
            if (timer == null)
                return;

            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            timer = null;

        }
        

        public static void UpdateTasks(object state)
        {
            UpdateRepeats();

            if (OnTasksUpdated != null)
                OnTasksUpdated.Invoke(); //GUI force reavaluate TaskItemViewModel's TaskColor property?
        }

        public static void UpdateRepeats()
        {
            //Update previous days
            var yesterday = DateTime.Today.AddDays(-1);
            
            //Testing Code
            /*
            foreach (var t in Workspace.Instance.Tasks.Where(x => x.Frequency == TaskFrequency.Daily))
            //&& x.DueDate == DateTime.Today
            {
                t.DueDate = yesterday;
            }

            Workspace.Instance.SaveChanges();
            */

            var matches = Workspace.Instance.Tasks
                .Where(x => x.Frequency != TaskFrequency.No 
                        && yesterday >= x.DueDate).ToList();

            foreach (var t in matches)
            {
                TaskLog log = new TaskLog() {
                    Date = yesterday,
                    TaskID = t.TaskItemID };

                log.Completed = t.Completed.HasValue;
                Workspace.Instance.TasksLog.Add(log);

                var next = Workspace.API.CalculateNextReminder(t.Frequency, t.DueDate.Value);
                t.DueDate = next;
                t.Completed = null;
            }

            Workspace.Instance.SaveChanges();
        }

        public static void UpdateTasks()
        {
            UpdateTasks(null);
        }
    }
}
