using System;

namespace Softeq.NetKit.Components.EventBus.Events
{
    public interface IIntegrationEvent
    {
        Guid Id { get; }

        DateTimeOffset Created { get; }
    }
}