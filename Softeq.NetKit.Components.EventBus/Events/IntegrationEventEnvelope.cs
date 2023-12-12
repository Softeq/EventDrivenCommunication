// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus.Events
{
    public sealed class IntegrationEventEnvelope
    {
        public IntegrationEventEnvelope(
            IntegrationEvent @event,
            string publisherId,
            string sequenceId = null,
            string correlationId = null)
            : this(Guid.NewGuid(), @event, publisherId, sequenceId, correlationId)
        {
        }

        private IntegrationEventEnvelope(
            Guid id,
            IntegrationEvent @event,
            string publisherId,
            string sequenceId = null,
            string correlationId = null)
        {
            Id = id;
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
            PublisherId = publisherId ?? throw new ArgumentNullException(nameof(publisherId));
            Created = DateTimeOffset.UtcNow;
            SequenceId = sequenceId;
            CorrelationId = correlationId;
        }

        public Guid Id { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public string PublisherId { get; private set; }
        public string SequenceId { get; private set; }
        public string CorrelationId { get; private set; }
        public IntegrationEvent Event { get; private set; }

        public static IntegrationEventEnvelope FromEnvelope<TEvent>(IntegrationEventEnvelope<TEvent> eventEnvelope)
            where TEvent : IntegrationEvent
        {
            return new IntegrationEventEnvelope(
                eventEnvelope.Id,
                eventEnvelope.Event,
                eventEnvelope.PublisherId,
                eventEnvelope.SequenceId,
                eventEnvelope.CorrelationId);
        }
    }
}