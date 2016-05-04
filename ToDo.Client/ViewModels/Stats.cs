namespace ToDo.Client.ViewModels
{
    public class Stats
    {
        public int Completed { get; private set; }
        public int Overdue { get; private set; }
        public int Remaining { get; private set; }
        public int Total { get; private set; }

        public Stats(int total, int completed, int remaining, int overdue)
        {
            this.Total = total;
            this.Completed = completed;
            this.Remaining = remaining;
            this.Overdue = overdue;
        }
    }
}