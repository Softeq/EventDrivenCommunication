// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Softeq.NetKit.Components.EventBus.Abstract;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Components.EventBus.Managers;
using Softeq.NetKit.Components.EventBus.Service.Connection;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Softeq.NetKit.Components.EventBus.Service
{
    public class EventBusService : IEventBusSubscriber, IEventBusPublisher
    {
        private readonly IEventBusSubscriptionsManager _subscriptionsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusTopicConnection _topicConnection;
        private readonly ServiceBusQueueConnection _queueConnection;
        private readonly ILogger _logger;
        private readonly EventPublishConfiguration _eventPublishConfiguration;

        private bool IsTopicSubscriptionAvailable => _topicConnection?.SubscriptionClient != null;
        private bool IsQueueAvailable => _queueConnection != null;

        public EventBusService(
            IServiceBusPersisterConnection serviceBusPersisterConnection,
            IEventBusSubscriptionsManager subscriptionsManager,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            EventPublishConfiguration eventPublishConfiguration)
        {
            _topicConnection = serviceBusPersisterConnection.TopicConnection;
            _queueConnection = serviceBusPersisterConnection.QueueConnection;
            _subscriptionsManager = subscriptionsManager;
            _serviceProvider = serviceProvider;
            _eventPublishConfiguration = eventPublishConfiguration;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task RegisterTopicEventListenerAsync()
        {
            ValidateSubscription();

            await RemoveDefaultRuleIfExists();

            _topicConnection.SubscriptionClient.RegisterMessageHandler(
                async (message, token) => await HandleReceivedMessage(
                    _topicConnection.SubscriptionClient,
                    _topicConnection.TopicClient,
                    message,
                    token),
                new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = 10, AutoComplete = false });

            async Task RemoveDefaultRuleIfExists()
            {
                try
                {
                    if (await CheckIfRuleExists(RuleDescription.DefaultRuleName))
                    {
                        await _topicConnection.SubscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName);
                    }
                }
                catch (MessagingEntityNotFoundException ex)
                {
                    throw new Exceptions.ServiceBusException(
                        $"Removing default rule {RuleDescription.DefaultRuleName} (if exists) failed.", ex);
                }
            }
        }

        public void RegisterQueueEventListener(QueueListenerConfiguration configuration = null)
        {
            ValidateQueue();

            var useSessions = configuration?.UseSessions ?? false;
            if (useSessions)
            {
                var handlerOptions = new SessionHandlerOptions(ExceptionReceivedHandler)
                {
                    AutoComplete = false,
                    MaxConcurrentSessions = configuration.MaxConcurrent
                };
                _queueConnection.QueueClient.RegisterSessionHandler(
                    async (session, message, token) =>
                        await HandleReceivedMessage(
                            session,
                            _topicConnection.TopicClient,
                            message,
                            token),
                    handlerOptions);
            }
            else
            {
                var handlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = configuration?.MaxConcurrent ?? 1,
                    AutoComplete = false
                };
                _queueConnection.QueueClient.RegisterMessageHandler(
                    async (message, token) =>
                        await HandleReceivedMessage(
                            _queueConnection.QueueClient,
                            _topicConnection.TopicClient,
                            message,
                            token),
                    handlerOptions);
            }
        }

        public async Task RegisterEventAsync<TEvent>() where TEvent : IntegrationEvent
        {
            var eventName = typeof(TEvent).Name;
            if (IsTopicSubscriptionAvailable)
            {
                await AddTopicSubscriptionRuleIfNotExists();
            }
            _subscriptionsManager.RegisterEventType<TEvent>();

            async Task AddTopicSubscriptionRuleIfNotExists()
            {
                try
                {
                    if (!await CheckIfRuleExists(eventName))
                    {
                        await _topicConnection.SubscriptionClient.AddRuleAsync(new RuleDescription
                        {
                            Filter = new CorrelationFilter { Label = eventName },
                            Name = eventName
                        });
                    }
                }
                catch (ServiceBusException ex)
                {
                    throw new Exceptions.ServiceBusException(
                        $"Adding subscription rule for the entity {eventName} failed.", ex);
                }
            }
        }

        public async Task RemoveEventRegistrationAsync<TEvent>() where TEvent : IntegrationEvent
        {
            if (IsTopicSubscriptionAvailable)
            {
                var eventName = typeof(TEvent).Name;
                await RemoveTopicSubscriptionRule(eventName);
            }
            _subscriptionsManager.RemoveEventType<TEvent>();

            async Task RemoveTopicSubscriptionRule(string eventName)
            {
                try
                {
                    await _topicConnection.SubscriptionClient.RemoveRuleAsync(eventName);
                }
                catch (MessagingEntityNotFoundException ex)
                {
                    throw new Exceptions.ServiceBusException(
                        $"The messaging entity {eventName} could not be found.", ex);
                }
            }
        }

        public Task PublishToTopicAsync(IntegrationEventEnvelope eventEnvelope)
        {
            ValidateTopic();
            return PublishEventAsync(eventEnvelope, _topicConnection.TopicClient);
        }

        public Task PublishToQueueAsync(IntegrationEventEnvelope eventEnvelope)
        {
            ValidateQueue();
            return PublishEventAsync(eventEnvelope, _queueConnection.QueueClient);
        }

        private Task PublishEventAsync(IntegrationEventEnvelope eventEnvelope, ISenderClient client)
        {
            var eventType = eventEnvelope.Event.GetType().Name;
            var eventEnvelopeJson = JsonConvert.SerializeObject(eventEnvelope);
            var eventEnvelopeBytes = Encoding.UTF8.GetBytes(eventEnvelopeJson);
            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = eventEnvelopeBytes,
                Label = eventType,
                CorrelationId = eventEnvelope.CorrelationId,
                SessionId = eventEnvelope.SequenceId
            };
            if (_eventPublishConfiguration.EventTimeToLive.HasValue)
            {
                message.TimeToLive = _eventPublishConfiguration.EventTimeToLive.Value;
            }
            return client.SendAsync(message);
        }

        private async Task HandleReceivedMessage(
            IReceiverClient receiverClient,
            ISenderClient senderClient,
            Message message,
            CancellationToken token)
        {
            var eventName = message.Label;
            if (!_subscriptionsManager.IsEventRegistered(eventName))
            {
                // Skip processing if event type is not registered
                return;
            }

            var eventType = _subscriptionsManager.GetEventTypeByName(eventName);
            var envelopeBody = Encoding.UTF8.GetString(message.Body);
            var eventEnvelopeType = typeof(IntegrationEventEnvelope<>).MakeGenericType(eventType);
            var eventEnvelope = (dynamic)JsonConvert.DeserializeObject(envelopeBody, eventEnvelopeType);
            if (eventEnvelope == null)
            {
                throw new InvalidOperationException(
                    $"Failed to parse received message '{eventName}'. Raw body: '{envelopeBody}'.");
            }

            await ProcessEventEnvelopeAsync(eventEnvelope);
            await receiverClient.CompleteAsync(message.SystemProperties.LockToken);
            if (_eventPublishConfiguration.SendCompletionEvent && eventType != typeof(CompletedEvent))
            {
                var completedEvent = new CompletedEvent(eventEnvelope.Id, eventEnvelope.PublisherId);
                var completedEventEnvelope = new IntegrationEventEnvelope(
                    completedEvent, _eventPublishConfiguration.EventPublisherId);
                await PublishEventAsync(completedEventEnvelope, senderClient);
            }
        }

        private async Task ProcessEventEnvelopeAsync<TEvent>(IntegrationEventEnvelope<TEvent> eventEnvelope)
            where TEvent : IntegrationEvent
        {
            var handlerType = typeof(IEventEnvelopeHandler<>).MakeGenericType(typeof(TEvent));
            using (var scope = _serviceProvider.CreateScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                await ((dynamic)handler).HandleAsync(eventEnvelope);
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _logger.LogInformation(
                $"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.\n" +
                "Exception context for troubleshooting:\n" +
                $"Endpoint: {context.Endpoint}\n" +
                $"Entity Path: {context.EntityPath}\n" +
                $"Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        private void ValidateTopic()
        {
            if (_topicConnection?.TopicClient == null)
            {
                throw new InvalidOperationException("Topic connection is not configured");
            }
        }

        private void ValidateSubscription()
        {
            if (!IsTopicSubscriptionAvailable)
            {
                throw new InvalidOperationException("Topic Subscription connection is not configured");
            }
        }

        private void ValidateQueue()
        {
            if (!IsQueueAvailable)
            {
                throw new InvalidOperationException("Queue connection is not configured");
            }
        }

        private async Task<bool> CheckIfRuleExists(string ruleName)
        {
            try
            {
                var rules = await _topicConnection.SubscriptionClient.GetRulesAsync();

                return rules != null
                       && rules.Any(rule =>
                           string.Equals(rule.Name, ruleName, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (ServiceBusException ex)
            {
                throw new Exceptions.ServiceBusException(
                    $"Checking rule {ruleName} existence failed.", ex);
            }
        }
    }
}