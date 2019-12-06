// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using System;

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public class ServiceBusTopicConnection
    {
        private readonly Func<ITopicClient> _topicClientFactory;
        private readonly Func<ISubscriptionClient> _subscriptionClientFactory;

        private ITopicClient _topicClient;
        private ISubscriptionClient _subscriptionClient;

        public ITopicClient TopicClient => _topicClient?.IsClosedOrClosing ?? true
            ? _topicClient = _topicClientFactory()
            : _topicClient;

        public ISubscriptionClient SubscriptionClient => _subscriptionClientFactory != null && (_subscriptionClient?.IsClosedOrClosing ?? true)
            ? _subscriptionClient = _subscriptionClientFactory()
            : _subscriptionClient;

        public ServiceBusTopicConnection(string connectionString, ServiceBusPersisterTopicConnectionConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration?.TopicName))
            {
                throw new ArgumentNullException(nameof(configuration.TopicName));
            }

            _topicClientFactory = () => CreateTopicClient(connectionString, configuration.TopicName);

            if (!string.IsNullOrWhiteSpace(configuration.SubscriptionName))
            {
                _subscriptionClientFactory = () => CreateSubscriptionClient(connectionString, configuration.TopicName, configuration.SubscriptionName);
            }
        }

        public ServiceBusTopicConnection(string namespaceName, TokenProvider tokenProvider, ServiceBusPersisterTopicConnectionConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration?.TopicName))
            {
                throw new ArgumentNullException(nameof(configuration.TopicName));
            }

            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                throw new ArgumentNullException(nameof(namespaceName));
            }

            var sbNamespace = $"sb://{namespaceName}.servicebus.windows.net/";
            var provider = tokenProvider ?? TokenProvider.CreateManagedIdentityTokenProvider();
            _topicClientFactory = () => CreateTopicClient(sbNamespace, configuration.TopicName, provider);

            if (!string.IsNullOrWhiteSpace(configuration.SubscriptionName))
            {
                _subscriptionClientFactory = () => CreateSubscriptionClient(sbNamespace, configuration.TopicName, configuration.SubscriptionName, provider);
            }
        }

        private static ITopicClient CreateTopicClient(string connectionString, string topicName)
        {
            var client = new TopicClient(connectionString, topicName, RetryPolicy.Default);
            return client;
        }

        private static ITopicClient CreateTopicClient(string namespaceName, string topicName, TokenProvider tokenProvider)
        {
            var client = new TopicClient(namespaceName, topicName, tokenProvider);
            return client;
        }

        private static ISubscriptionClient CreateSubscriptionClient(string connectionString, string topicName, string subscriptionName)
        {
            var client = new SubscriptionClient(connectionString, topicName, subscriptionName);
            return client;
        }

        private static ISubscriptionClient CreateSubscriptionClient(string namespaceName, string topicName, string subscriptionName, TokenProvider tokenProvider)
        {
            var client = new SubscriptionClient(namespaceName, topicName, subscriptionName, tokenProvider);
            return client;
        }
    }
}