// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Components.EventBus.Managers
{
    public interface IEventBusSubscriptionsManager
    {
        void RegisterEventType<TEvent>() where TEvent : IntegrationEvent;
        void RemoveEventType<TEvent>() where TEvent : IntegrationEvent;
        bool IsEventRegistered(string eventName);
        Type GetEventTypeByName(string eventName);
    }
}