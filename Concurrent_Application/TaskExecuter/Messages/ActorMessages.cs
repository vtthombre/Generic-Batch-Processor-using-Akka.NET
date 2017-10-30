using Akka.Actor;
using System;

namespace TaskExecuter.Messages
{
    public abstract class JobMessage
    {
        public string Description { get; private set; }
        public int ID { get; private set; }
        protected JobMessage(string taskFileName, int taskId)
        {
            Description = taskFileName;
            ID = taskId;
        }
    }

    public class CanAcceptJobMessage : JobMessage
    {
        public CanAcceptJobMessage(string taskFileName, int taskId)
            : base(taskFileName, taskId) { }
    }

    public class AbleToAcceptJobMessage : JobMessage
    {
        public AbleToAcceptJobMessage(string taskFileName, int taskId)
            : base(taskFileName, taskId) { }
    }

    public class UnableToAcceptJobMessage : JobMessage
    {
        public UnableToAcceptJobMessage(string taskFileName, int taskId)
            : base(taskFileName, taskId) { }
    }

    public class JobStartedMessage : JobMessage
    {
        public DateTime ProcessedTime { get; private set; }
        public JobStartedMessage(string taskFileName, int taskId)
            : base(taskFileName, taskId)
        {
            ProcessedTime = DateTime.Now;
        }
    }

    public class JobFailedMessage : JobMessage
    {
        public JobStatus Status { get; private set; }
        public JobFailedMessage(string taskFileName, int taskId, JobStatus reason)
            : base(taskFileName, taskId)
        {
            Status = reason;
        }
    }

    public class JobCompletedMessage : JobMessage
    {
        public long Duration { get; private set; }
        public DateTime ProcessedTime { get; private set; }
        public JobCompletedMessage(string taskFileName, int taskId, long duration)
            : base(taskFileName, taskId)
        {
            Duration = duration;
            ProcessedTime = DateTime.Now;
        }
    }


    public class ProcessJobMessage : JobMessage
    {
        public IActorRef Client { get; set; }
        public ProcessJobMessage(string taskFileName, int taskId, IActorRef client = null)
            : base(taskFileName, taskId)
        {
            Client = client;
        }
    }

    public class ProcessStashedJobsMessage : JobMessage
    {
        public ProcessStashedJobsMessage(string taskFileName, int taskId)
            : base(taskFileName, taskId) { }
    }

    public class ProcessFileMessage
    {
        public string FileName { get; private set; }

        public ProcessFileMessage(string fileName)
        {
            FileName = fileName;
        }
    }

    public class JobValidationSucceedMessage { };
    public class JobValidationFailedMessage { };
}
