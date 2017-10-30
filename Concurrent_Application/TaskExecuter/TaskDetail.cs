using System;
namespace TaskExecuter
{
    internal class TaskDetail
    {
        public int ID { get; private set; }
        public string Description { get; private set; }
        public string MachineNode { get; set; }
        public JobStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long Duration { get; set; }

        internal TaskDetail(int taskId, string taskDescription)
        {
            ID = taskId;
            Description = taskDescription;
            Status = JobStatus.NotStarted;
            MachineNode = "localhost";
        }
    }
}
