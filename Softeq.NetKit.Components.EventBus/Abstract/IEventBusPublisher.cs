// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Components.EventBus.Events;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Softeq.NetKit.Components.EventBus.Abstract
{
    public interface IEventBusPublisher
    {
        Task PublishToTopicAsync(IntegrationEvent @event, int? delayInSeconds = null);

        Task PublishToTopicAsync(Message message, int? delayInSeconds = null);

        Task PublishToQueueAsync(IntegrationEvent @event, int? delayInSeconds = null);

        Task PublishToQueueAsync(Message message, int? delayInSeconds = null);
    }
}