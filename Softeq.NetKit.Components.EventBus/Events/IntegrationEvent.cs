using System;
using Newtonsoft.Json;

namespace Softeq.NetKit.Components.EventBus.Events
{
    public abstract class IntegrationEvent : IIntegrationEvent
    {
        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            Created = DateTimeOffset.UtcNow;
        }

        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        public DateTimeOffset Created { get; set; }
    }
}