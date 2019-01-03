// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.Azure.ServiceBus;
using System;

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public class ServiceBusQueueConnection
    {
        private readonly string _connectionString;
        private readonly ServiceBusPersisterQueueConnectionConfiguration _configuration;
        private IQueueClient _queueClient;

        public IQueueClient QueueClient => _queueClient.IsClosedOrClosing
            ? _queueClient = CreateClient(_connectionString, _configuration.QueueName)
            : _queueClient;

        public ServiceBusQueueConnection(string connectionString, ServiceBusPersisterQueueConnectionConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.QueueName))
            {
                throw new ArgumentNullException(nameof(configuration.QueueName));
            }

            _connectionString = connectionString;
            _configuration = configuration;
            _queueClient = CreateClient(connectionString, configuration.QueueName);
        }

        private IQueueClient CreateClient(string connectionString, string queueName)
        {
            var client = new QueueClient(connectionString, queueName, ReceiveMode.PeekLock, RetryPolicy.Default);
            return client;
        }
    }
}