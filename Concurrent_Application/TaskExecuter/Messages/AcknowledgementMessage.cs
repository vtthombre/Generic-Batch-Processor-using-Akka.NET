namespace TaskExecuter.Messages
{
    /// <summary>
    /// Enum for AcknowledgementReceipt
    /// </summary>
    public enum AcknowledgementReceipt
    {
        /// <summary>
        /// Task completed Successfully
        /// </summary>
        SUCCESS,

        // Task is failed due to unhandeled exception
        FAILED,

        /// <summary>
        /// Task is cancelled
        /// </summary>
        CANCELED,

        /// <summary>
        /// Task is cancelled due to time out
        /// </summary>
        TIMEOUT,

        /// <summary>
        /// Invalid task processed
        /// </summary>
        INVALID_TASK
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AcknowledgementMessage"/>  class
    /// </summary>
    public class AcknowledgementMessage
    {        
        /// <summary>
        /// Gets or sets the ID for the task 
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Gets or sets the description of the task
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or set the CompletionTime for the task
        /// </summary>
        public long CompletionTime { get; private set; }

        /// <summary>
        /// gets or sets the acknowledgement receipt
        /// </summary>
        public AcknowledgementReceipt Receipt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcknowledgementMessage"/>  class
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="duration"></param>
        /// <param name="receipt"></param>
        public AcknowledgementMessage(int id, string description, long duration,
            AcknowledgementReceipt receipt)
        {
            ID = id;
            Description = description;
            CompletionTime = duration;
            Receipt = receipt;
        }
    }
}
