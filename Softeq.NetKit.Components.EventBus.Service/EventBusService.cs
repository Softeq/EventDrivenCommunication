// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Softeq.NetKit.Components.EventBus.Abstract;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Components.EventBus.Managers;
using Softeq.NetKit.Components.EventBus.Service.Connection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace Softeq.NetKit.Components.EventBus.Service
{
    public class EventBusService : IEventBusPublisher, IEventBusSubscriber
    {
        private readonly IEventBusSubscriptionsManager _subscriptionsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusTopicConnection _topicConnection;
        private readonly ServiceBusQueueConnection _queueConnection;
        private readonly ILogger _logger;
        private readonly EventPublisherConfiguration _eventPublisherConfiguration;
        private readonly AsyncRetryPolicy _publishMessageRetryPolicy;

        private bool IsSubscriptionAvailable => _topicConnection?.SubscriptionClient != null;
        private bool IsQueueAvailable => _queueConnection != null;

        public EventBusService(
            IServiceBusPersisterConnection serviceBusPersisterConnection,
            IEventBusSubscriptionsManager subscriptionsManager,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            EventPublisherConfiguration eventPublisherConfiguration)
        {
            _topicConnection = serviceBusPersisterConnection.TopicConnection;
            _queueConnection = serviceBusPersisterConnection.QueueConnection;
            _subscriptionsManager = subscriptionsManager;
            _serviceProvider = serviceProvider;
            _eventPublisherConfiguration = eventPublisherConfiguration;
            _logger = loggerFactory.CreateLogger(GetType());
            _publishMessageRetryPolicy = Policy
                .Handle<ServiceBusException>(ex => ex.IsTransient)
                .WaitAndRetryAsync(sleepDurations: Backoff.ExponentialBackoff(TimeSpan.FromSeconds(1), 3));
        }

        public Task PublishToTopicAsync(IntegrationEvent @event, int? delayInSeconds = null)
        {
            ValidateTopic();
            return PublishEventAsync(@event, _topicConnection.TopicClient, delayInSeconds);
        }

        public Task PublishToQueueAsync(IntegrationEvent @event, int? delayInSeconds = null)
        {
            ValidateQueue();
            return PublishEventAsync(@event, _queueConnection.QueueClient, delayInSeconds);
        }

        public async Task SubscribeAsync<TEvent, TEventHandler>() where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;

            var containsKey = _subscriptionsManager.HasSubscriptionsForEvent<TEvent>();
            if (!containsKey)
            {
                if (IsSubscriptionAvailable)
                {
                    await AddSubscriptionRuleIfNotExists(eventName);
                }

                _subscriptionsManager.AddSubscription<TEvent, TEventHandler>();
            }
        }

        public async Task UnsubscribeAsync<TEvent, TEventHandler>() where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;

            if (IsSubscriptionAvailable)
            {
                await RemoveSubscriptionRule(eventName);
            }

            _subscriptionsManager.RemoveSubscription<TEvent, TEventHandler>();
        }

        public void SubscribeDynamic<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
        {
            _subscriptionsManager.AddDynamicSubscription<TEventHandler>(eventName);
        }

        public void UnsubscribeDynamic<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
        {
            _subscriptionsManager.RemoveDynamicSubscription<TEventHandler>(eventName);
        }

        public async Task RegisterSubscriptionListenerAsync(SubscriptionListenerConfiguration configuration)
        {
            Ensure.Any.IsNotNull(configuration, nameof(configuration));
            ValidateSubscription();

            await RemoveDefaultRuleIfExists();

            _topicConnection.SubscriptionClient.RegisterMessageHandler(
                async (message, token) => await HandleReceivedMessage(
                    _topicConnection.SubscriptionClient, 
                    _topicConnection.TopicClient, 
                    message),
                new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = configuration.MaxConcurrentCalls,
                    AutoComplete = false
                });

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

        public void RegisterQueueListener(QueueListenerConfiguration configuration)
        {
            Ensure.Any.IsNotNull(configuration, nameof(configuration));
            ValidateQueue();

            if (configuration.UseSessions)
            {
                var handlerOptions = new SessionHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentSessions = configuration.MaxConcurrent,
                    AutoComplete = false
                };
                _queueConnection.QueueClient.RegisterSessionHandler(
                    async (session, message, token) => await HandleReceivedMessage(
                        session,
                        _topicConnection.TopicClient,
                        message),
                    handlerOptions);
            }
            else
            {
                var handlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = configuration.MaxConcurrent,
                    AutoComplete = false
                };
                _queueConnection.QueueClient.RegisterMessageHandler(
                     async (message, token) => await HandleReceivedMessage(
                         _queueConnection.QueueClient,
                         _queueConnection.QueueClient,
                         message),
                     handlerOptions);
            }
        }

        private Task PublishEventAsync(IntegrationEvent @event, ISenderClient client, int? delayInSeconds = null)
        {
            return PublishMessageAsync(() => GetMessageForPublish(@event), client, delayInSeconds);
        }

        private Task PublishMessageAsync(
            Func<Message> messageFactory, 
            ISenderClient client, 
            int? delayInSeconds = null)
        {
            return _publishMessageRetryPolicy.ExecuteAsync(() =>
            {
                var message = messageFactory();
                return delayInSeconds.HasValue
                        ? client.ScheduleMessageAsync(message, DateTime.UtcNow.AddSeconds(delayInSeconds.Value))
                        : client.SendAsync(message);
            });
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

        private async Task AddSubscriptionRuleIfNotExists(string eventName)
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

        private async Task RemoveSubscriptionRule(string eventName)
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

        private async Task HandleReceivedMessage(
            IReceiverClient receiverClient,
            ISenderClient senderClient,
            Message message)
        {
            try
            {
                await HandleMessageAsync(senderClient, message);
            }
            finally
            {
                // Complete the message to prevent it from being moved to the Dead Letter Queue even in case of a processing failure.
                // Not completed event will be republished by the EDC background job.
                await receiverClient.CompleteAsync(message.SystemProperties.LockToken);
            }
        }

        private async Task HandleMessageAsync(
            ISenderClient senderClient,
            Message message)
        {
            var eventName = message.Label;
            var messageBody = Encoding.UTF8.GetString(message.Body);

            await ProcessEventAsync();

            if (_eventPublisherConfiguration.SendCompletionEvent)
            {
                await SendCompletionEvent();
            }

            return;

            async Task ProcessEventAsync()
            {
                if (!_subscriptionsManager.HasSubscriptionsForEvent(eventName))
                {
                    return;
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _subscriptionsManager.GetEventHandlers(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(subscription.HandlerType);

                        if (subscription.IsDynamic && handler is IDynamicEventHandler eventHandler)
                        {
                            dynamic eventData = JObject.Parse(messageBody);
                            await eventHandler.Handle(eventData);
                        }
                        else if (handler != null)
                        {
                            var eventType = _subscriptionsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(messageBody, eventType);
                            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType
                                .GetMethod(nameof(IEventHandler<IntegrationEvent>.Handle))
                                .Invoke(handler, new[] { integrationEvent });
                        }
                    }
                }
            }

            async Task SendCompletionEvent()
            {
                var eventType = _subscriptionsManager.GetEventTypeByName(eventName);
                if (eventType != null
                    && eventType != typeof(CompletedEvent))
                {
                    var eventData = JObject.Parse(messageBody);
                    if (Guid.TryParse((string)eventData["Id"], out var eventId))
                    {
                        var publisherId = (string)eventData["PublisherId"];
                        var completedEvent = new CompletedEvent(eventId, publisherId);
                        await PublishEventAsync(completedEvent, senderClient);
                    }
                }
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

        private Message GetMessageForPublish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = eventName,
                CorrelationId = @event.CorrelationId,
                SessionId = @event.SessionId
            };
            if (_eventPublisherConfiguration.MessageTimeToLive.HasValue)
            {
                message.TimeToLive = _eventPublisherConfiguration.MessageTimeToLive.Value;
            }
            return message;
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
            if (!IsSubscriptionAvailable)
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