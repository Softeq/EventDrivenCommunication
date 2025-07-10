// Developed by Softeq Development Corporation
// http://www.softeq.com

using EnsureThat;

namespace Softeq.NetKit.Components.EventBus
{
    public class EventPublishConfiguration
    {
        public EventPublishConfiguration(
            string eventPublisherId,
            bool sendCompletionEvent = true)
        {
            EventPublisherId = Ensure.String.IsNotNullOrEmpty(eventPublisherId, nameof(eventPublisherId));
            SendCompletionEvent = sendCompletionEvent;
        }

        /// <summary>
        /// A unique identifier for the service or component publishing the events.
        /// Used to track the origin of events in a distributed system.
        /// </summary>
        public string EventPublisherId { get; }

        /// <summary>
        /// Indicates whether a <see cref="Events.CompletedEvent"/> should be published 
        /// after the processing of the original event is finished.
        /// </summary>
        public bool SendCompletionEvent { get; }
    }
}
