// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.NetKit.Integrations.EventLog
{
    /// <summary>
    /// Represents the current state of an event in the event log lifecycle.
    /// </summary>
    public enum EventState
    {
        /// <summary>
        /// Event has been created but not yet published.
        /// </summary>
        NotPublished = 0,

        /// <summary>
        /// Event has been successfully published.
        /// </summary>
        Published = 1,

        /// <summary>
        /// Event was published, but a completion acknowledgment has not been received.
        /// This may indicate a timeout or communication issue.
        /// </summary>
        PublishAcknowledgmentTimeout = 2,

        /// <summary>
        /// Event has been processed and completed (successfully or with failure).
        /// </summary>
        Completed = 3
    }
}