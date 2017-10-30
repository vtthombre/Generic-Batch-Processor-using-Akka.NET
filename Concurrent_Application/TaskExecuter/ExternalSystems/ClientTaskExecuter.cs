using System.Threading.Tasks;
using TaskExecuter.Messages;

namespace TaskExecuter.ExternalSystems
{
    public class ClientTaskExecuter : ITaskExecuter
    {
        public async Task<AcknowledgementMessage> ExecuteTask(JobStartedMessage taskMessage)
        {
            return await Task.Delay(100)
               .ContinueWith<AcknowledgementMessage>(task =>
               {
                  
                   AcknowledgementReceipt receipt = AcknowledgementReceipt.SUCCESS;
                   long taskTime = 0;
                   Task externalTask = Task.Factory.StartNew(() =>
                   {
                       // call or execute an external application here
                   });

                   // wait for task to complete
                   externalTask.Wait();

                   switch (externalTask.Status)
                   {
                       case TaskStatus.Faulted:
                           receipt = AcknowledgementReceipt.FAILED;
                           break;
                       case TaskStatus.Canceled:
                           receipt = AcknowledgementReceipt.CANCELED;
                           break;
                       case TaskStatus.RanToCompletion:
                           receipt = AcknowledgementReceipt.SUCCESS;
                           break;
                   }

                   // send the acknowledgement
                   return new AcknowledgementMessage(taskMessage.ID, taskMessage.Description, taskTime, receipt);
               });
        }

    }
}
