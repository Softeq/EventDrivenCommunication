// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus
{
    public class EventPublishConfiguration
    {
        public EventPublishConfiguration(
            string eventPublisherId,
            bool sendCompletionEvent = true,
            TimeSpan? eventTimeToLive = null)
        {
            EventPublisherId = eventPublisherId;
            SendCompletionEvent = sendCompletionEvent;
            EventTimeToLive = eventTimeToLive;
        }

        public string EventPublisherId { get; }

        public bool SendCompletionEvent { get; }

        public TimeSpan? EventTimeToLive { get; }
    }
}
