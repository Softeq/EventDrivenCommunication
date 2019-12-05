// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Components.EventBus.Abstract
{
    public interface IEventBusSubscriber
    {
        void RegisterQueueListener(QueueListenerConfiguration configuration = null);

        Task RegisterSubscriptionListenerAsync();

        Task SubscribeAsync<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>;

        Task UnsubscribeAsync<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>;

        void SubscribeDynamic<TEventHandler>(string eventName)
            where TEventHandler : IDynamicEventHandler;

        void UnsubscribeDynamic<TEventHandler>(string eventName)
            where TEventHandler : IDynamicEventHandler;
    }
}