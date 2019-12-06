// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public abstract class ServiceBusPersisterConnectionConfiguration
    {
        public ServiceBusPersisterTopicConnectionConfiguration TopicConfiguration { get; set; }
        public ServiceBusPersisterQueueConnectionConfiguration QueueConfiguration { get; set; }
    }

    public class ServiceBusPersisterTopicConnectionConfiguration
    {
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
    }

    public class ServiceBusPersisterQueueConnectionConfiguration
    {
        public string QueueName { get; set; }
    }
}