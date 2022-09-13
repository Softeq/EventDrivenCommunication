// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using System;
using System.Threading.Tasks;

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public class ServiceBusQueueConnection : IAsyncDisposable
    {
        private IQueueClient _queueClient;
        private readonly Func<IQueueClient> _queueClientFactory;

        public IQueueClient QueueClient => _queueClient?.IsClosedOrClosing ?? true
            ? _queueClient = _queueClientFactory()
            : _queueClient;

        public ServiceBusQueueConnection(string connectionString, ServiceBusPersisterQueueConnectionConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration?.QueueName))
            {
                throw new ArgumentNullException(nameof(configuration.QueueName));
            }

            _queueClientFactory = () => CreateClient(connectionString, configuration.QueueName);
        }

        public ServiceBusQueueConnection(string namespaceName, TokenProvider tokenProvider, ServiceBusPersisterQueueConnectionConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration?.QueueName))
            {
                throw new ArgumentNullException(nameof(configuration.QueueName));
            }

            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                throw new ArgumentNullException(nameof(namespaceName));
            }

            _queueClientFactory = () => CreateClient(namespaceName, configuration.QueueName, tokenProvider);
        }

        public async ValueTask DisposeAsync()
        {
            if (_queueClient != null && !_queueClient.IsClosedOrClosing)
            {
                await _queueClient.CloseAsync();
            }
        }

        private static IQueueClient CreateClient(string connectionString, string queueName)
        {
            var client = new QueueClient(connectionString, queueName, ReceiveMode.PeekLock, RetryPolicy.Default);
            return client;
        }

        private static IQueueClient CreateClient(string namespaceName, string queueName, TokenProvider tokenProvider)
        {
            var sbNamespace = $"sb://{namespaceName}.servicebus.windows.net/";
            var provider = tokenProvider ?? TokenProvider.CreateManagedIdentityTokenProvider();

            var client = new QueueClient(sbNamespace, queueName, provider);
            return client;
        }
    }
}