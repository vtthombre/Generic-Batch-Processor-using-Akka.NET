# Generic Batch Processor using Akka.NET
 ” Building a concurrent, remote and distributed System for batch processing which is fault tolerant and can scale up or scale out using [Akka.NET](http://getakka.net/ "Akka.NET - .NET distributed actor framework") (based on actor model)”. 

A generic batch processor is used for dividing parallel work to multiple actors on the same machine, remote machine as well as on distributed network.

Batch Processor workflow
![Image of Workflow](https://github.com/vtthombre/Generic-Batch-Processor-using-Akka.NET/blob/master/Batchprocessor_Workflow.PNG)

## Job Manager
Job Manger is managing all the tasks and their status. This can be UI or console based appliaction.
### Job Pool Controller Actor
Job Pool Controller actor is a part of Job Manager. It has following responsibilities
1. Assign the task to commander actor
2. Get the response from commander actor
3. update the job status
4. print the job statastics
 
### Job Scheduler
Job Scheduler is a part of Job Manager. It is responsible for scheduling the job after each interval (10 sec ). It is also responsible for checking the job status after every 3 minutes.



