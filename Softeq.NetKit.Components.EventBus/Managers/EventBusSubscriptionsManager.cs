// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using Softeq.NetKit.Components.EventBus.Abstract;
using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Components.EventBus.Managers
{
    public class EventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        private readonly Dictionary<string, List<EventSubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public EventBusSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<EventSubscriptionInfo>>();
            _eventTypes = new List<Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public void AddDynamicSubscription<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
        {
            DoAddSubscription(typeof(TEventHandler), eventName, true);
        }

        public void AddSubscription<TEvent, TEventHandler>() where TEvent : IntegrationEvent where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = GetEventKey<TEvent>();
            DoAddSubscription(typeof(TEventHandler), eventName, false);
            _eventTypes.Add(typeof(TEvent));
        }

        public void RemoveSubscription<TEvent, TEventHandler>() where TEvent : IntegrationEvent where TEventHandler : IEventHandler<TEvent>
        {
            var handlerToRemove = FindSubscriptionToRemove<TEvent, TEventHandler>();
            var eventName = GetEventKey<TEvent>();
            DoRemoveSubscription(eventName, handlerToRemove);
        }

        public void RemoveDynamicSubscription<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
        {
            var handlerToRemove = FindDynamicSubscriptionToRemove<TEventHandler>(eventName);
            DoRemoveSubscription(eventName, handlerToRemove);
        }

        private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<EventSubscriptionInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(isDynamic ? EventSubscriptionInfo.Dynamic(handlerType) : EventSubscriptionInfo.Typed(handlerType));
        }

        private void DoRemoveSubscription(string eventName, EventSubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[eventName].Remove(subsToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            if (handler != null)
            {
                OnEventRemoved?.Invoke(this, eventName);
            }
        }

        private EventSubscriptionInfo FindDynamicSubscriptionToRemove<TEventHandler>(string eventName)
            where TEventHandler : IDynamicEventHandler
        {
            return DoFindSubscriptionToRemove(eventName, typeof(TEventHandler));
        }

        private EventSubscriptionInfo FindSubscriptionToRemove<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = GetEventKey<TEvent>();
            return DoFindSubscriptionToRemove(eventName, typeof(TEventHandler));
        }

        private EventSubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            return HasSubscriptionsForEvent(eventName) 
                ? _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType) 
                : null;
        }
        public bool HasSubscriptionsForEvent<TEvent>() where TEvent : IntegrationEvent
        {
            var key = GetEventKey<TEvent>();
            return HasSubscriptionsForEvent(key);
        }

        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(x => x.Name == eventName);

        public IEnumerable<EventSubscriptionInfo> GetEventHandlers<TEvent>() where TEvent : IntegrationEvent
        {
            var key = GetEventKey<TEvent>();
            return GetEventHandlers(key);
        }

        public IEnumerable<EventSubscriptionInfo> GetEventHandlers(string eventName)
        {
            return _handlers.TryGetValue(eventName, out var subscriptions)
                ? (IEnumerable<EventSubscriptionInfo>) subscriptions
                : Array.Empty<EventSubscriptionInfo>();
        }

        public string GetEventKey<TEvent>()
        {
            return typeof(TEvent).Name;
        }
    }
}
