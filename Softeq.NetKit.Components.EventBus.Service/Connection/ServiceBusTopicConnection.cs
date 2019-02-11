// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.Azure.ServiceBus;
using System;

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public class ServiceBusTopicConnection
    {
        private readonly string _connectionString;
        private readonly ServiceBusPersisterTopicConnectionConfiguration _configuration;
        private ITopicClient _topicClient;
        private ISubscriptionClient _subscriptionClient;

        public ITopicClient TopicClient => _topicClient.IsClosedOrClosing
            ? _topicClient = CreateTopicClient(_connectionString, _configuration.TopicName)
            : _topicClient;

        public ISubscriptionClient SubscriptionClient => _subscriptionClient != null &&_subscriptionClient.IsClosedOrClosing
            ? _subscriptionClient = CreateSubscriptionClient(_connectionString, _configuration.TopicName, _configuration.SubscriptionName)
            : _subscriptionClient;

        public ServiceBusTopicConnection(string connectionString, ServiceBusPersisterTopicConnectionConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.TopicName))
            {
                throw new ArgumentNullException(nameof(configuration.TopicName));
            }

            _connectionString = connectionString;
            _configuration = configuration;
            _topicClient = CreateTopicClient(connectionString, configuration.TopicName);

            if (!string.IsNullOrWhiteSpace(configuration.SubscriptionName))
            {
                _subscriptionClient = CreateSubscriptionClient(connectionString, configuration.TopicName, configuration.SubscriptionName);
            }
        }

        private static ITopicClient CreateTopicClient(string connectionString, string topicName)
        {
            var client = new TopicClient(connectionString, topicName, RetryPolicy.Default);
            return client;
        }

        private static ISubscriptionClient CreateSubscriptionClient(string connectionString, string topicName, string subscriptionName)
        {
            var client = new SubscriptionClient(connectionString, topicName, subscriptionName);
            return client;
        }
    }
}