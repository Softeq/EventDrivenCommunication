// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus.Events
{
    public abstract class IntegrationEvent
    {
        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; private set; } //Do not remove 'private set' so compiler won't drop backing field setter
        public DateTimeOffset CreationDate { get; private set; } //Do not remove 'private set' so compiler won't drop backing field setter
        public string PublisherId { get; set; }
        public string CorrelationId { get; set; }
        public string SessionId { get; set; }
    }
}