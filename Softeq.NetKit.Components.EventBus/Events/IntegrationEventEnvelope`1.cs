// Developed by Softeq Development Corporation
// http://www.softeq.com

using Newtonsoft.Json;
using System;

namespace Softeq.NetKit.Components.EventBus.Events
{
    public class IntegrationEventEnvelope<TEvent> where TEvent : IntegrationEvent
    {
        [JsonConstructor]
        private IntegrationEventEnvelope(
            Guid id, 
            DateTimeOffset created, 
            string publisherId, 
            string sequenceId, 
            string correlationId,
            TEvent @event)
        {
            Id = id;
            Created = created;
            PublisherId = publisherId;
            SequenceId = sequenceId;
            CorrelationId = correlationId;
            Event = @event;
        }

        public IntegrationEventEnvelope(IntegrationEventEnvelope eventEnvelope)
        {
            if (eventEnvelope == null)
            {
                throw new ArgumentNullException(nameof(eventEnvelope));
            }
            
            Event = eventEnvelope.Event as TEvent ?? throw new ArgumentException(nameof(eventEnvelope.Event));
            Id = eventEnvelope.Id;
            Created = eventEnvelope.Created;
            PublisherId = eventEnvelope.PublisherId;
            SequenceId = eventEnvelope.SequenceId;
            CorrelationId = eventEnvelope.CorrelationId;
        }

        public Guid Id { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public string PublisherId { get; private set; }
        public string SequenceId { get; private set; }
        public string CorrelationId { get; private set; }
        public TEvent Event { get; }
    }
}