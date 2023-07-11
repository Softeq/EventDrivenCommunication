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
    public class EventBusService : IEventBusPublisher, IEventBusSubscriber
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

        public async Task RegisterTopicListenerAsync()
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

        public void RegisterQueueListener(QueueListenerConfiguration configuration = null)
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

        public async Task RegisterTopicEventAsync<TEvent>() where TEvent : IntegrationEvent
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

        public async Task RemoveTopicEventRegistrationAsync<TEvent>() where TEvent : IntegrationEvent
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

        public void RegisterQueueEventAsync<TEvent>() where TEvent : IntegrationEvent
        {
            _subscriptionsManager.RegisterEventType<TEvent>();
        }

        public void RemoveQueueEventRegistrationAsync<TEvent>() where TEvent : IntegrationEvent
        {
            _subscriptionsManager.RemoveEventType<TEvent>();
        }

        public Task PublishToTopicAsync(IntegrationEventEnvelope @event)
        {
            ValidateTopic();
            return PublishEventAsync(@event, _topicConnection.TopicClient);
        }

        public Task PublishToQueueAsync(IntegrationEventEnvelope @event)
        {
            ValidateQueue();
            return PublishEventAsync(@event, _queueConnection.QueueClient);
        }

        private Task PublishEventAsync(IntegrationEventEnvelope integrationEventEnvelope, ISenderClient client)
        {
            //if (string.IsNullOrEmpty(integrationEventV2.PublisherId))
            //{
            //    integrationEventV2.PublisherId = _eventPublishConfiguration.EventPublisherId;
            //}

            var eventName = integrationEventEnvelope.Event.GetType().Name;
            var jsonMessage = JsonConvert.SerializeObject(integrationEventEnvelope);
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = eventName,
                CorrelationId = integrationEventEnvelope.CorrelationId,
                SessionId = integrationEventEnvelope.SessionId
            };
            if (_eventPublishConfiguration.EventTimeToLive.HasValue)
            {
                message.TimeToLive = _eventPublishConfiguration.EventTimeToLive.Value;
            }
            return client.SendAsync(message);
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

        private async Task HandleReceivedMessage(
            IReceiverClient receiverClient,
            ISenderClient senderClient,
            Message message,
            CancellationToken token)
        {
            var eventName = message.Label;
            var eventType = _subscriptionsManager.GetEventTypeByName(eventName);
            var envelope = ParseEventEnvelopeAsync(message);
            await ProcessEvent(eventName, envelope);
            await receiverClient.CompleteAsync(message.SystemProperties.LockToken);
            if (_eventPublishConfiguration.SendCompletionEvent && eventType != typeof(CompletedEvent))
            {
                var completedEvent = new CompletedEvent(envelope.Id, envelope.PublisherId);
                var completedEventEnvelope = new IntegrationEventEnvelope(_eventPublishConfiguration.EventPublisherId, completedEvent);
                await PublishEventAsync(completedEventEnvelope, senderClient);
            }
        }

        private IntegrationEventEnvelope ParseEventEnvelopeAsync(Message message)
        {
            var eventName = message.Label;
            var eventType = _subscriptionsManager.GetEventTypeByName(eventName);
            var envelopeBody = Encoding.UTF8.GetString(message.Body);
            var envelope = (IntegrationEventEnvelope)JsonConvert.DeserializeObject(envelopeBody, eventType);
            if (envelope == null)
            {
                throw new InvalidOperationException($"Failed to parse received message '{eventName}'. Raw body: '{envelopeBody}'.");
            }
            envelope.Event.Id = envelope.Id;
            envelope.Event.Created = envelope.Created;
            return envelope;
        }

        private async Task ProcessEvent(string eventName, IntegrationEventEnvelope integrationEventEnvelope)
        {
            if (!_subscriptionsManager.IsEventRegistered(eventName))
            {
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEventEnvelope.Event.GetType());
                var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                await HandleParsedEventAsync((dynamic)handler, (dynamic)integrationEventEnvelope.Event);
            }
        }

        private static async Task HandleParsedEventAsync<TEventContent>(IIntegrationEventHandler<TEventContent> handler, TEventContent content)
            where TEventContent : IntegrationEvent
        {
            await handler.HandleAsync(content);
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
    }
}