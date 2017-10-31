# Generic Batch Processor using Akka.NET
 ” Building a concurrent, remote and distributed System for batch processing which is fault tolerant and can scale up or scale out using [Akka.NET](http://getakka.net/ "Akka.NET - .NET distributed actor framework") (based on actor model)”. 

A generic batch processor is used for dividing parallel work to multiple actors on the same machine, remote machine as well as on distributed network.

### Batch Processor workflow

![Image of Workflow](https://github.com/vtthombre/Generic-Batch-Processor-using-Akka.NET/blob/master/Batchprocessor_Workflow.PNG)


## Job Manager
Job Manger is managing all the tasks and their status. This can be UI or console based appliaction.
### Job Pool Controller Actor
Job Pool Controller actor has following responsibilities
1. Assign the task to commander actor
2. Get the response from commander actor
3. update the job status
4. print the job statastics
 
### Job Scheduler
Job Scheduler is responsible for scheduling the job after each interval (10 sec ). It is also responsible for checking the job status after every 3 minutes.


## Executer
Executer is the backbone the batchprocessor acgtor system. It is responsible for taking the job from job manager (Job pool controller actor) and perfor that job and after completion, update job manager.
### Commander Actor
Commander actor has following responsibilities-
1. Create the broadcast router for creating coordinator instances.
2. Get the task from Job pool controller and assign it to any available coordinator.
3. Get the response from coordinator and update to job pool controller about the task.

### Coordinator Actor
Coordinator actor plays mediator role between commander and worker actor.Coordinator actor has following responsibilities-
1. Create the worker actor for performing task.
2. Supervise worker actor. If worker actor failed to perform any task then it will take necessary action.
3. Update the commander once the task is completed by worker.


### Worker Actor
Worker actor is the last actor in the hierarchy which actully perform the task. It has following responsibilities-
1. Get the task from coordinator and perform the task.
2. Update the task status to coordinator

## Current Samples
**[Concurrent application for Batch Processing](/Concurrent_Application/)** - how to execute multiple tasks concurrently as well as paralley in Akka.NET.

## Contributing

Please feel free to contribute.

### Questions about Samples?

Please [create a Github issue](https://github.com/vtthombre/Generic-Batch-Processor-using-Akka.NET/issues) for any questions you might have.

### Code License


## About Author






