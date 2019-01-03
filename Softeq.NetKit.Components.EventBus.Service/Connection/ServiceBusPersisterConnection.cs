// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public class ServiceBusPersisterConnection : IServiceBusPersisterConnection
    {
        private readonly ServiceBusPersisterConnectionConfiguration _configuration;

        public string ServiceBusConnectionString => _configuration.ConnectionString;

        public ServiceBusTopicConnection TopicConnection { get; }
        public ServiceBusQueueConnection QueueConnection { get; }

        public ServiceBusPersisterConnection(ServiceBusPersisterConnectionConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (configuration.TopicConfiguration != null)
                TopicConnection = new ServiceBusTopicConnection(configuration.ConnectionString, configuration.TopicConfiguration);

            if (configuration.QueueConfiguration != null)
                QueueConnection = new ServiceBusQueueConnection(configuration.ConnectionString, configuration.QueueConfiguration);
        }
    }
}
