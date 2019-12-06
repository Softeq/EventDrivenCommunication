// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Microsoft.Azure.ServiceBus.Core;

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public class ServiceBusPersisterConnection : IServiceBusPersisterConnection
    {
        public ServiceBusPersisterConnection(ServiceBusPersisterConnectionConfiguration configuration)
        {
            ConnectionConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            switch (configuration)
            {
                case ServiceBusConnectionStringConnectionConfiguration stringConfiguration:
                    CreateConnectionStringConnections(stringConfiguration);
                    break;
                case ServiceBusMsiConnectionConfiguration msiConfiguration:
                    CreateMsiConnections(msiConfiguration);
                    break;
                default:
                    throw new NotSupportedException($"Configuration of type {configuration.GetType().Name} not supported");
            }
        }

        public ServiceBusPersisterConnectionConfiguration ConnectionConfiguration { get; }
        public ServiceBusTopicConnection TopicConnection { get; private set; }
        public ServiceBusQueueConnection QueueConnection { get; private set; }

        public MessageReceiver CreateMessageReceiver(string entity)
        {
            switch (ConnectionConfiguration)
            {
                case ServiceBusConnectionStringConnectionConfiguration stringConfiguration:
                    return new MessageReceiver(stringConfiguration.ConnectionString, entity);
                case ServiceBusMsiConnectionConfiguration msiConfiguration:
                    return new MessageReceiver(msiConfiguration.NamespaceName, entity, msiConfiguration.TokenProvider);
                default:
                    throw new NotSupportedException($"Configuration of type {ConnectionConfiguration.GetType().Name} not supported");
            }
        }

        private void CreateConnectionStringConnections(ServiceBusConnectionStringConnectionConfiguration configuration)
        {
            if (configuration.QueueConfiguration != null)
            {
                QueueConnection = new ServiceBusQueueConnection(configuration.ConnectionString, configuration.QueueConfiguration);
            }

            if (configuration.TopicConfiguration != null)
            {
                TopicConnection = new ServiceBusTopicConnection(configuration.ConnectionString, configuration.TopicConfiguration);
            }
        }

        private void CreateMsiConnections(ServiceBusMsiConnectionConfiguration configuration)
        {
            if (configuration.QueueConfiguration != null)
            {
                QueueConnection = new ServiceBusQueueConnection(configuration.NamespaceName, configuration.TokenProvider, configuration.QueueConfiguration);
            }

            if (configuration.TopicConfiguration != null)
            {
                TopicConnection = new ServiceBusTopicConnection(configuration.NamespaceName, configuration.TokenProvider, configuration.TopicConfiguration);
            }
        }
    }
}