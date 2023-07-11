// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Components.EventBus.Managers
{
    public class EventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        private readonly List<Type> _eventTypes = new List<Type>();

        public void RegisterEventType<TEvent>() where TEvent : IntegrationEvent
        {
            var eventType = typeof(TEvent);
            if (_eventTypes.Contains(eventType))
            {
                throw new ArgumentException($"Event {GetEventName<TEvent>()} is already registered.");
            }
            _eventTypes.Add(typeof(TEvent));
        }

        public void RemoveEventType<TEvent>() where TEvent : IntegrationEvent
        {
            var eventType = typeof(TEvent);
            if (!_eventTypes.Contains(eventType))
            {
                throw new ArgumentException($"Event {GetEventName<TEvent>()} is not registered.");
            }
            _eventTypes.Remove(typeof(TEvent));
        }

        public bool IsEventRegistered<TEvent>() where TEvent : IntegrationEvent
        {
            var eventName = GetEventName<TEvent>();
            return IsEventRegistered(eventName);
        }

        public bool IsEventRegistered(string eventName) => _eventTypes.Any(x => x.Name == eventName);

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(x => x.Name == eventName);

        private static string GetEventName<TEvent>()
        {
            return typeof(TEvent).Name;
        }
    }
}