// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public interface IServiceBusPersisterConnection
    {
        string ServiceBusConnectionString { get; }
        ServiceBusTopicConnection TopicConnection { get; }
        ServiceBusQueueConnection QueueConnection { get; }
    }
}
