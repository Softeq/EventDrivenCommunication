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

        public Guid Id { get; }
        public DateTimeOffset CreationDate { get; }
    }
}
