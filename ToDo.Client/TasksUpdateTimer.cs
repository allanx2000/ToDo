using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        

        private static void UpdateTasks(object state)
        {
            UpdateRepeats();

            if (OnTasksUpdated != null)
                OnTasksUpdated.Invoke(); //GUI force reavaluate TaskItemViewModel's TaskColor property?
        }

        public static void UpdateRepeats()
        {
            //Update previous days
            var yesterday = DateTime.Today.AddDays(-1);

            var matches = Workspace.Instance.Tasks.Where(x => yesterday == x.NextReminder);

            foreach (var t in matches)
            {
                t.NextReminder = Workspace.API.CalculateNextReminder(t.Frequency.Value, t.NextReminder.Value);
                t.Completed = null;
            }

            Workspace.Instance.SaveChanges();
        }
    }
}
