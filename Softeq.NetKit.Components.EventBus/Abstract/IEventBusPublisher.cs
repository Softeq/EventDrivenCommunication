// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Components.EventBus.Events;
using System.Threading.Tasks;

namespace Softeq.NetKit.Components.EventBus.Abstract
{
    public interface IEventBusPublisher
    {
        Task PublishToTopicAsync(IntegrationEventEnvelope eventEnvelope);
        Task PublishToQueueAsync(IntegrationEventEnvelope eventEnvelope);
    }
}