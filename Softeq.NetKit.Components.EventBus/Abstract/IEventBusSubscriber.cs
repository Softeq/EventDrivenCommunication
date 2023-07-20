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
        Task RegisterEventAsync<TEvent>() where TEvent : IntegrationEvent;
        Task RemoveEventRegistrationAsync<TEvent>() where TEvent : IntegrationEvent;
    }
}