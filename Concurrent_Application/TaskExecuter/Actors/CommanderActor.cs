using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using System;
using System.Linq;
using TaskExecuter.Messages;

namespace TaskExecuter.Actors
{
    /// <summary>
    /// Top-level actor responsible for coordinating and launching task-processing jobs
    /// </summary>
    public class CommanderActor : ReceiveActor
    {
        #region private members

        /// <summary>
        /// Coordinator instance actor
        /// </summary>
        private IActorRef _coordinator;
     
        /// <summary>
        /// Logger object to log instructions into log file.
        /// </summary>
        private ILoggingAdapter _logger = Context.GetLogger();

        /// <summary>
        /// no. of routees replies
        /// </summary>
        private int _pendingJobReplies;

        /// <summary>
        /// current job to be processed
        /// </summary>
        private ProcessJobMessage _currentJob;
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CommanderActor"/>  class
        /// </summary>
        public CommanderActor()
        {
            _logger.Debug("Commander instance created.");
            Ready();
        }

        #region Switchable behaviour for Coordinator
        private void Ready()
        {
            ColorConsole.WriteLineGreen("Commander's current state is Ready.");
            Receive<ProcessJobMessage>(job =>
            {
                ColorConsole.WriteLineWhite($"Commander has received Task {job.ID}.{job.Description} for processing.");
                _currentJob = job;

                // ask the coordinator for job
                _coordinator.Tell(new CanAcceptJobMessage(job.Description, job.ID));

                // move to next state              
                BecomeAsking();
            });
         

            Receive<JobCompletedMessage>(job =>
            {
                // send response to client
                _currentJob.Client.Tell(job);
                
                _logger.Debug($"Task {job.ID} is completed by commander and {Sender.Path.Name}.");
            });

            Receive<JobFailedMessage>(job =>
            {
                // send response to client
                _currentJob.Client.Tell(job);

                ColorConsole.WriteLineGreen($"Task {job.ID} is failed.");                
            });           
        }

        private void BecomeAsking()
        {
            _logger.Debug("Commander's current state is BecomeAsking.");

            // block, but ask the router for the number of routees. Avoids magic numbers.
            _pendingJobReplies = _coordinator.Ask<Routees>(new GetRoutees()).Result.Members.Count();

            // move to next state
            Become(Asking);

            // send ourselves a ReceiveTimeout message if no message within 3 seonds
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(3));
        }

        private void Asking()
        {
            _logger.Debug("Commander's current state is Asking.");           

            // received UnableToAcceptJobMessage from coordinator
            Receive<UnableToAcceptJobMessage>(job =>
            {
                _logger.Debug($"Commander has received UnableToAcceptJobMessage from { Sender.Path.Name} for Task ID: {job.ID}");

                // each routee is giving response to cordinator
                _pendingJobReplies--;
                if (_pendingJobReplies == 0)
                {
                    // send response to client
                    _currentJob.Client.Tell(job);

                    // move to next state
                    BecomeReady();
                }
            });

            // received AbleToAcceptJobMessage from coordinator
            Receive<AbleToAcceptJobMessage>(job =>
            {
                ColorConsole.WriteLineCyan($"Starting Job for Task ID: {job.ID}. {job.Description}");
                _logger.Debug($"Commander has received AbleToAcceptJobMessage from {Sender.Path.Name} for Task ID: {job.ID}");

                var jobMessage = new JobStartedMessage(job.Description, job.ID);
                Sender.Tell(jobMessage, Self);

                // tell the client that job has been started
                _currentJob.Client.Tell(jobMessage);

                BecomeReady();
            });

            // means at least one actor failed to respond
            Receive<ReceiveTimeout>(timeout =>
            {
                _logger.Warning("Commander has received ReceiveTimeout from {0}", Sender.Path.Name);
                
                // move to next state
                BecomeReady();
            });
        }

        private void BecomeReady()
        {
            _logger.Debug("Commander's current state is BecomeReady.");
            // move to next state
            Become(Ready);

            // cancel ReceiveTimeout
            Context.SetReceiveTimeout(null);
        }

        #endregion
              
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                exception =>
                {
                    ColorConsole.WriteLineRed("Unknown exception caught, Restarting the task...");
                    _logger.Error("Unknown exception caught, Restarting the task...");
                    return Directive.Restart;
                });
        }

        #region Lifecycle Event Hooks
        protected override void PreStart()
        {
            _logger.Debug("Commander's PreStart called.");

            // create a broadcast router who will ask all of them if they're available for work
            _logger.Info("Creating coordinators instances using broadcast router.");
            _coordinator = Context.ActorOf(Props.Create(() => new CoordinatorActor())
                .WithRouter(FromConfig.Instance), ActorPaths.CoordinatorActor.Name);            
        
            base.PreStart();
        }

        protected override void PostStop()
        {
            _logger.Debug("Commander's PostStop called.");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteLineWhite("Commander's PreRestart called because: {0} ", reason.Message);
            _logger.Warning("Commander's PreRestart called because: {0} ", reason.Message);

            //kill off the old coordinator so we can recreate it from scratch
            //_logger.Info("kill off the old coordinator so we can recreate it from scratch");
            //ColorConsole.WriteLineWhite("kill off the old coordinator so we can recreate it from scratch");
            _coordinator.Tell(PoisonPill.Instance);

            base.PreRestart(reason, message);
        }

        #endregion
    }
}
