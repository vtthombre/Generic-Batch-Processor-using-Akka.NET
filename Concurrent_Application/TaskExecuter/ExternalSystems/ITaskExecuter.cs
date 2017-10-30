using System.Threading.Tasks;
using TaskExecuter.Messages;

namespace TaskExecuter.ExternalSystems
{
    /// <summary>
    /// Interface for client executable tasks
    /// </summary>
    public interface ITaskExecuter
    {
        Task<AcknowledgementMessage> ExecuteTask(JobStartedMessage task);
    }
}
