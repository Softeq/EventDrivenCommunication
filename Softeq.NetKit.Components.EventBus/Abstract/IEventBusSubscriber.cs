// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Components.EventBus.Abstract
{
    public interface IEventBusSubscriber
    {
        Task RegisterTopicListenerAsync();
        void RegisterQueueListener(QueueListenerConfiguration configuration = null);
        Task RegisterTopicEventAsync<TEvent>() where TEvent : IntegrationEvent;
        Task RemoveTopicEventRegistrationAsync<TEvent>() where TEvent : IntegrationEvent;
        void RegisterQueueEventAsync<TEvent>() where TEvent : IntegrationEvent;
        void RemoveQueueEventRegistrationAsync<TEvent>() where TEvent : IntegrationEvent;
    }
}