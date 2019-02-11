// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus.Events
{
    public class CompletedEvent : IntegrationEvent
    {
        public CompletedEvent(Guid completedEventId, string completedEventPublisherId)
        {
            CompletedEventId = completedEventId;
            CompletedEventPublisherId = completedEventPublisherId;
        }

        public Guid CompletedEventId { get; }

        public string CompletedEventPublisherId { get; }
    }
}
