// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus.Events
{
    public sealed class IntegrationEventEnvelope
    {
        public IntegrationEventEnvelope(
            string publisherId,
            IntegrationEvent @event,
            string sessionId = null,
            string correlationId = null)
        {
            PublisherId = publisherId ?? throw new ArgumentNullException(nameof(publisherId));
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
            Id = @event.Id;
            Created = @event.Created;
            SessionId = sessionId;
            CorrelationId = correlationId;
        }

        public Guid Id { get; private set; }

        public DateTimeOffset Created { get; private set; }

        public string PublisherId { get; private set; }

        public string SessionId { get; private set; }

        public string CorrelationId { get; private set; }

        public IntegrationEvent Event { get; private set; }
    }
}