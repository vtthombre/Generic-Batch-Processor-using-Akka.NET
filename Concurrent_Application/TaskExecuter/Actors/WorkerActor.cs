using Akka.Actor;
using Akka.Event;
using System;
using TaskExecuter.Exceptions;
using TaskExecuter.ExternalSystems;
using TaskExecuter.Messages;

namespace TaskExecuter.Actors
{
    internal class WorkerActor: ReceiveActor
    {
        #region private members
        /// <summary>
        /// Logger object to log instructions into log file.
        /// </summary>
        private ILoggingAdapter _logger = Context.GetLogger();

        /// <summary>
        /// executer instance
        /// </summary>
        private readonly ITaskExecuter _taskExecuter;
       
        /// <summary>
        /// instance of BeginJobMessage
        /// </summary>
        private JobStartedMessage _myJob;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerActor"/>  class
        /// </summary>
        /// <param name="executer">Executer object</param>
        public WorkerActor(ITaskExecuter executer)
        {            
            _taskExecuter = executer;

            Receive<JobStartedMessage>(job => HandleJobExecute(job));
            Receive<AcknowledgementMessage>(message => HandleAcknowldgement(message));

            _logger.Debug("Worker instance created.");
        }

        #region Handle Receive Messages
        /// <summary>
        /// Perform the exceution of job, Handles "BeginJobMessage" message
        /// </summary>
        /// <param name="job"></param>
        private void HandleJobExecute(JobStartedMessage job)
        {
            _logger.Info("Worker has received BeginJobMessage from {0} for Task ID: {1}", Sender.Path.Name, job.ID);
           
            _myJob = job;         

            _logger.Info("Worker has started Task ID: {0}", job.ID);
            // use pipeto to handle async call from caller ( taskexecuter )
            _taskExecuter.ExecuteTask(job).PipeTo(Self, Sender);
        }

        /// <summary>
        /// Perform acknowledgement message from the sender.
        /// </summary>
        /// <param name="message"></param>
        private void HandleAcknowldgement(AcknowledgementMessage message)
        {
            if (message.Receipt == AcknowledgementReceipt.CANCELED)
            {
                _logger.Warning("User has cancelled the operation for Task ID: {0}.", message.ID);
                throw new JobCanceledException();
            }
            else if (message.Receipt == AcknowledgementReceipt.INVALID_TASK)
            {
                Context.Parent.Tell(new JobFailedMessage(message.Description, message.ID, JobStatus.Failed));
                ColorConsole.WriteLineRed($"Task {message.ID}. {message.Description} is invalid.");
                _logger.Warning($"Task {message.ID}. {message.Description} is invalid.");
            }
            else if (message.Receipt == AcknowledgementReceipt.FAILED)
            {
                Context.Parent.Tell(new JobFailedMessage(message.Description, message.ID, JobStatus.Failed));
                ColorConsole.WriteLineRed($"Task {message.ID}. { message.Description} is failed due to unhandled exeption.");
                _logger.Warning("Unhandled exception caught from external application for Task ID: {0}", message.ID);
            }
            else if (message.Receipt == AcknowledgementReceipt.TIMEOUT)
            {
                Context.Parent.Tell(new JobFailedMessage(message.Description, message.ID, JobStatus.Timeout));
                ColorConsole.WriteLineRed("Task ID: {0} is cancelled due to time out error.", message.ID);
                _logger.Error("Task ID: {0} is cancelled due to time out error.", message.ID);
            }            
            else
            {
                if (message.CompletionTime == 0)
                {
                    Context.Parent.Tell(new JobFailedMessage(message.Description, message.ID, JobStatus.Cancelled));
                    ColorConsole.WriteLineCyan($"Task ID: {message.ID} failed to execute by external application.");
                    _logger.Error($"Task ID: {message.ID} failed to execute by external application.");
                }
                else
                {
                    Context.Parent.Tell(new JobCompletedMessage(message.Description, message.ID, message.CompletionTime));
                    ColorConsole.WriteLineCyan($"Task ID: {message.ID} completed successfully by worker.");                    
                    _logger.Info($"Task ID: {message.ID} completed successfully by worker.");
                }
            }
        }
        #endregion

        #region Lifecycle hooks

        protected override void PreStart()
        {
            _logger.Debug("WorkerActor for Coordinator {0} called PreStart", Context.Parent.Path.Name);
        }

        protected override void PostStop()
        {
            ColorConsole.WriteLineRed("WorkerActor for Coordinator {0} called PostStop.", Context.Parent.Path.Name);
            _logger.Error("WorkerActor for Coordinator {0} called PostStop.", Context.Parent.Path.Name);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteLineWhite("WorkerActor for Coordinator {0} called PreReStart because: {1}", Context.Parent.Path.Name, reason.Message);
            _logger.Info("Restarting Task ID: {0} of Coordinator {0} because: {1}", _myJob.ID, Context.Parent.Path.Name, reason.Message);
            Self.Tell(_myJob);
        }

        protected override void PostRestart(Exception reason)
        {
            _logger.Warning("WorkerActor for Coordinator {0} PostReStart because: {1}", Context.Parent.Path.Name, reason.Message);
            base.PostRestart(reason);
        }
        #endregion
    }
}
