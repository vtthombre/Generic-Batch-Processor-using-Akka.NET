# Generic Batch Processor using Akka.NET
 ” Building a concurrent, remote and distributed System for batch processing which is fault tolerant and can scale up or scale out using [Akka.NET](http://getakka.net/ "Akka.NET - .NET distributed actor framework") (based on actor model)”. 

A generic batch processor is used for dividing parallel work to multiple actors on the same machine, remote machine as well as on distributed network.

Batch Processor workflow



## Detailed Explaination about Actor
# Job Pool Controller Actor
Job pool controller actor is part of Job Manager. It has following responsibilities
1. Assign the task to commander actor
2. Get the response from commander actor
3. update the job status
4. print the job statastics





