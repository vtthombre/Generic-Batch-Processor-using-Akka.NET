using Akka.Actor;
using System.Collections.Generic;
using TaskExecuter.Messages;
using System;
using System.IO;
using Akka.Event;
using System.Linq;
using System.Diagnostics;

namespace TaskExecuter.Actors
{
    public class JobPoolControllerActor : ReceiveActor
    {
        private class ProcessUnfinishedJobs { }

        Dictionary<int, ProcessJobMessage> _jobsToProcessed = new Dictionary<int, ProcessJobMessage>();
        Dictionary<int, TaskDetail> _taskList = new Dictionary<int, TaskDetail>();
       
        /// <summary>
        /// scheduler instance for processing unstashed objects
        /// </summary>
        private ICancelable _jobScheduler;

        /// <summary>
        /// commander actor instance
        /// </summary>
        private IActorRef _commander;

        /// <summary>
        /// timeout of task in minutes
        /// </summary>
        int _taskTimeout = 3;

        /// <summary>
        /// stop watch to calculate time for processing all jobs 
        /// </summary>
        Stopwatch _stopWatch;

        /// <summary>
        /// Logger object to log instructions into log file.
        /// </summary>
        private ILoggingAdapter _logger = Context.GetLogger();

        public JobPoolControllerActor(IActorRef commanderActor)
        {
            _commander = commanderActor;
            Receive<ProcessFileMessage>(msg => InitializeJobs(msg.FileName));
            Receive<UnableToAcceptJobMessage>(job =>
            {
                ColorConsole.WriteLineRed($"Commander {Sender.Path} is unable to perform Task : {job.Description}");
                if (!_jobsToProcessed.ContainsKey(job.ID))
                    _jobsToProcessed.Add(job.ID, new ProcessJobMessage(job.Description, job.ID, Self));
            });

            Receive<JobStartedMessage>(job =>
            {
                ColorConsole.WriteLineGreen($"Task {job.ID}. {job.Description} has started by {Sender.Path} at {job.ProcessedTime}");
                var task = _taskList[job.ID];             
                task.StartTime = job.ProcessedTime;
                task.Status = JobStatus.Started;
                if (null == _stopWatch)
                {
                    _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                }
                // update UI
            });

            Receive<JobCompletedMessage>(job =>
            {
                ColorConsole.WriteLineYellow($"Task {job.ID}. {job.Description} has completed succesfully at {job.ProcessedTime} by {Sender.Path} in {job.Duration} ms.");
                var task = _taskList[job.ID];
                task.EndTime = job.ProcessedTime;
                task.Duration = job.Duration;
                task.Status = JobStatus.Completed;
                // update UI
            });

            Receive<JobFailedMessage>(job =>
            {
                ColorConsole.WriteLineRed($"Commander {Sender.Path} is unable to perform Task : {job.Description} because {job.Status.ToString()}");
                _taskList[job.ID].Status = job.Status;
                // update UI
            });

            Receive<ProcessUnfinishedJobs>(msg => HandleProcessUnFinishedJobs(msg));
        }

        private void HandleProcessJob()
        {
            Context.System.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), () =>
            {
                if (_jobsToProcessed.Count > 0)
                {
                    var currentJobMsg = _jobsToProcessed.Values.ElementAt(0);
                    _commander.Tell(currentJobMsg);
                    _jobsToProcessed.Remove(currentJobMsg.ID);
                }
            });
        }
        private void HandleProcessUnFinishedJobs(ProcessUnfinishedJobs msg)
        {
            if (_jobsToProcessed.Count() == 0)
            {
                var count = _taskList.Values.Where(x => x.Status == JobStatus.Completed).Count();
                if (count == _taskList.Count())
                {
                    PrintSummary();
                    if (null != _stopWatch)
                    {
                        long totalProcessedTime = _stopWatch.ElapsedMilliseconds;                        
                        TimeSpan taskDuration = TimeSpan.FromMilliseconds(totalProcessedTime);
                        ColorConsole.WriteLineCyan($"Total time to process all jobs = {taskDuration.ToString(@"hh\:mm\:ss\:fff")}");
                        _stopWatch.Stop();
                    }
                    _jobScheduler.Cancel();
                }
                else
                {
                    List<TaskDetail> failedtasks = _taskList.Values.Where(x => x.Status == JobStatus.Cancelled 
                        || x.Status == JobStatus.Timeout || x.Status == JobStatus.NotStarted
                        ||(x.Status == JobStatus.Started && DateTime.Now.Subtract(x.StartTime).TotalMinutes > _taskTimeout)
                        ).ToList();

                    if (failedtasks.Count() > 0)
                    {
                        PrintSummary();
                        ColorConsole.WriteLineBlue("Processing unfinished jobs...");

                        foreach (TaskDetail task in failedtasks)
                        {
                            if (!_jobsToProcessed.ContainsKey(task.ID))
                                _jobsToProcessed.Add(task.ID, new ProcessJobMessage(task.Description, task.ID, Self));
                        }
                    }
                    else
                    {
                        List<TaskDetail> unFinishedtasks = _taskList.Values.Where(x => x.Status == JobStatus.Started
                                && DateTime.Now.Subtract(x.StartTime).TotalMinutes < _taskTimeout).ToList();
                        if (unFinishedtasks.Count() == 0)
                        {
                            PrintSummary();
                            _jobScheduler.Cancel();

                            List<TaskDetail> failedJobs = _taskList.Values.Where(x => x.Status == JobStatus.Failed ||
                                        x.Status == JobStatus.InvalidTask).ToList();
                            PrintFailedJobsSummary(failedJobs);
                        }
                    }
                }
            }
        }

        private void PrintSummary()
        {
            ColorConsole.WriteLineWhite("==================================================================================================");
            ColorConsole.WriteLineWhite("                                        Job Summary                                               ");
            ColorConsole.WriteLineWhite("==================================================================================================");

            ColorConsole.WriteLineWhite(" ID       Task Description         Node       Status    Start Time     End Time      Duration");
            ColorConsole.WriteLineWhite("--------------------------------------------------------------------------------------------------");
            foreach (var task in _taskList.Values)
            {
                TimeSpan taskDuration = TimeSpan.FromMilliseconds(task.Duration);
                ColorConsole.WriteLineYellow($"Task {task.ID} | {task.Description} | {task.MachineNode} | {task.Status.ToString()} | {task.StartTime.ToString("hh:mm:ss tt")} | {task.EndTime.ToString("hh:mm:ss tt")} | {taskDuration.ToString(@"hh\:mm\:ss\:fff")}");
            }

            ColorConsole.WriteLineWhite("==================================================================================================");
        }

        private void PrintFailedJobsSummary(List<TaskDetail> filedJobs)
        {
            if (filedJobs.Count() > 0)
            {
                ColorConsole.WriteLineWhite("============================================================================");
                ColorConsole.WriteLineWhite("           Failed Job Summary                                               ");
                ColorConsole.WriteLineWhite("============================================================================");

                ColorConsole.WriteLineWhite(" ID       Task Description         Status          Reason                   ");
                ColorConsole.WriteLineWhite("----------------------------------------------------------------------------");
                foreach (var task in filedJobs)
                {
                    ColorConsole.WriteLineYellow($"Task {task.ID} | {task.Description} | {task.Status.ToString()} | {task.Status.GetStringValue()}");
                }

                ColorConsole.WriteLineWhite("============================================================================");
            }
        }
        private void InitializeJobs(string fileName)
        {
            ParseTxtFile(fileName);
            ColorConsole.WriteLineCyan("Initializing Job Pool...");
            ColorConsole.WriteLineWhite("====================================");
            foreach (var job in _jobsToProcessed.Values)
            {
                ColorConsole.WriteLineCyan("Task {0}. {1}", job.ID, job.Description);
            }

            ColorConsole.WriteLineWhite("------------------------------------");
            ColorConsole.WriteLineCyan("Total tasks to be processed are : {0}", _jobsToProcessed.Count);
            ColorConsole.WriteLineWhite("====================================");

            HandleProcessJob();
        }

        private void ParseTxtFile(string fileName)
        {
            bool fileFound = File.Exists(fileName);
            if (!fileFound)
            {
                fileName = Path.Combine(Environment.CurrentDirectory, fileName);
                fileFound = File.Exists(fileName);
                if (!fileFound)
                {
                    ColorConsole.WriteLineRed("Task file not found.");
                    return;
                }
            }
                       
            var fileLines = File.ReadAllLines(fileName);
            int jobId = 0;
            foreach (var line in fileLines)
            {
                jobId++;
                var job = new ProcessJobMessage(line, jobId, Self);
                _jobsToProcessed.Add(jobId, job);
                _taskList.Add(jobId, new TaskDetail(jobId, line));
            }            
        }
        
        #region Lifecycle Event Hooks
        protected override void PreStart()
        {
            _jobScheduler = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                 TimeSpan.FromMinutes(2), // in minutes
                 TimeSpan.FromSeconds(10),
                 Self,
                 new ProcessUnfinishedJobs(),
                 Self);
            base.PreStart();
        }
        
        protected override void PostStop()
        {
            _jobScheduler.Cancel();
        }

        #endregion
    }
}
