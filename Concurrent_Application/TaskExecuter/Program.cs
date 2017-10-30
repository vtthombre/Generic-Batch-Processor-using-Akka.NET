using System;
using Akka.Actor;
using Akka.DI.Core;
using Akka.DI.Ninject;
using Ninject;
using TaskExecuter.Actors;
using TaskExecuter.ExternalSystems;
using TaskExecuter.Messages;
using System.Threading;

namespace TaskExecuter
{
    internal class Program
    {
        private static ActorSystem TaskExecuterActorSystem;

        private static void Main(string[] args)
        {
            ColorConsole.WriteLineGray("Creating Batch processor system");
            CreateActorSystem();
           
            IActorRef commanderActor = TaskExecuterActorSystem.ActorOf(Props.Create<CommanderActor>(),
              ActorPaths.CommanderActor.Name);

            IActorRef jobPoolControllerActor = TaskExecuterActorSystem.ActorOf(Props.Create<JobPoolControllerActor>(commanderActor),
               ActorPaths.JobPoolControllerActor.Name); 

            jobPoolControllerActor.Tell(new ProcessFileMessage("JobPool.txt"));

            TaskExecuterActorSystem.WhenTerminated.Wait();
        }


        private static void CreateActorSystem()
        {
            // Ninject Dependency Injector
            var container = new StandardKernel();
            container.Bind<ITaskExecuter>().To<ClientTaskExecuter>();
            container.Bind<WorkerActor>().ToSelf();

            TaskExecuterActorSystem = ActorSystem.Create("batchprocessor");

            IDependencyResolver resolver = new NinjectDependencyResolver(container, TaskExecuterActorSystem);
        }       
    }
}
