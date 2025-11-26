// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Components.EventBus.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Softeq.NetKit.Components.EventBus.Abstract
{
    public interface IEventBusPublisher
    {
        Task PublishToTopicAsync(IntegrationEvent @event, int? delayInSeconds = null);
        Task PublishToTopicAsync(IList<IntegrationEvent> events);
        Task PublishToQueueAsync(IntegrationEvent @event, int? delayInSeconds = null);
        Task PublishToQueueAsync(IList<IntegrationEvent> events);
    }
}