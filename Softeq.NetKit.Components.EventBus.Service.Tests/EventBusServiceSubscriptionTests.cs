// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Softeq.NetKit.Components.EventBus.Abstract;
using Softeq.NetKit.Components.EventBus.Managers;
using Softeq.NetKit.Components.EventBus.Service.Connection;
using Softeq.NetKit.Components.EventBus.Service.Tests.Samples.Events;
using Xunit;

namespace Softeq.NetKit.Components.EventBus.Service.Tests
{
    public class EventBusServiceSubscriptionTests
    {
        private const string EventName = nameof(AccountRegisteredEvent);

        private readonly EventBusSubscriptionsManager _subscriptionsManager = new EventBusSubscriptionsManager();
        private readonly HandledEvents _handledEvents = new HandledEvents();
        private readonly Mock<ISubscriptionClient> _subscriptionClient = new Mock<ISubscriptionClient>();

        [Fact]
        public async Task SubscribeAsync_TwoHandlersForSameEvent_RegistersBoth()
        {
            var eventBus = CreateEventBus(withTopicSubscription: false);

            await eventBus.SubscribeAsync<AccountRegisteredEvent, FirstHandler>();
            await eventBus.SubscribeAsync<AccountRegisteredEvent, SecondHandler>();

            var handlerTypes = _subscriptionsManager.GetEventHandlers(EventName).Select(x => x.HandlerType).ToList();
            Assert.Equal(new[] { typeof(FirstHandler), typeof(SecondHandler) }, handlerTypes);
            Assert.Equal(typeof(AccountRegisteredEvent), _subscriptionsManager.GetEventTypeByName(EventName));
        }

        [Fact]
        public async Task SubscribeAsync_SameHandlerTwice_RegistersItOnce()
        {
            var eventBus = CreateEventBus(withTopicSubscription: true);

            await eventBus.SubscribeAsync<AccountRegisteredEvent, FirstHandler>();
            await eventBus.SubscribeAsync<AccountRegisteredEvent, FirstHandler>();

            Assert.Single(_subscriptionsManager.GetEventHandlers(EventName));
            _subscriptionClient.Verify(x => x.AddRuleAsync(It.IsAny<RuleDescription>()), Times.Once);
        }

        [Fact]
        public async Task SubscribeAsync_EventAlreadyHasDynamicHandler_RegistersTypedHandler()
        {
            var eventBus = CreateEventBus(withTopicSubscription: false);

            eventBus.SubscribeDynamic<DynamicHandler>(EventName);
            await eventBus.SubscribeAsync<AccountRegisteredEvent, FirstHandler>();

            Assert.Contains(_subscriptionsManager.GetEventHandlers(EventName), x => x.HandlerType == typeof(FirstHandler));
        }

        [Fact]
        public async Task SubscribeAsync_TwoHandlersForSameEvent_AddsSingleSubscriptionRule()
        {
            var eventBus = CreateEventBus(withTopicSubscription: true);

            await eventBus.SubscribeAsync<AccountRegisteredEvent, FirstHandler>();
            await eventBus.SubscribeAsync<AccountRegisteredEvent, SecondHandler>();

            _subscriptionClient.Verify(x => x.AddRuleAsync(It.Is<RuleDescription>(r => r.Name == EventName)), Times.Once);
        }

        [Fact]
        public async Task UnsubscribeAsync_OtherHandlerStillSubscribed_KeepsSubscriptionRule()
        {
            var eventBus = CreateEventBus(withTopicSubscription: true);
            await eventBus.SubscribeAsync<AccountRegisteredEvent, FirstHandler>();
            await eventBus.SubscribeAsync<AccountRegisteredEvent, SecondHandler>();

            await eventBus.UnsubscribeAsync<AccountRegisteredEvent, FirstHandler>();

            _subscriptionClient.Verify(x => x.RemoveRuleAsync(It.IsAny<string>()), Times.Never);
            Assert.Equal(typeof(SecondHandler), _subscriptionsManager.GetEventHandlers(EventName).Single().HandlerType);
        }

        [Fact]
        public async Task UnsubscribeAsync_LastHandler_RemovesSubscriptionRule()
        {
            var eventBus = CreateEventBus(withTopicSubscription: true);
            await eventBus.SubscribeAsync<AccountRegisteredEvent, FirstHandler>();
            await eventBus.SubscribeAsync<AccountRegisteredEvent, SecondHandler>();

            await eventBus.UnsubscribeAsync<AccountRegisteredEvent, FirstHandler>();
            await eventBus.UnsubscribeAsync<AccountRegisteredEvent, SecondHandler>();

            _subscriptionClient.Verify(x => x.RemoveRuleAsync(EventName), Times.Once);
            Assert.False(_subscriptionsManager.HasSubscriptionsForEvent(EventName));
        }

        [Fact]
        public async Task ReceivedMessage_TwoHandlersForSameEvent_InvokesBoth()
        {
            Func<Message, CancellationToken, Task> messageHandler = null;
            _subscriptionClient
                .Setup(x => x.RegisterMessageHandler(It.IsAny<Func<Message, CancellationToken, Task>>(), It.IsAny<MessageHandlerOptions>()))
                .Callback<Func<Message, CancellationToken, Task>, MessageHandlerOptions>((handler, options) => messageHandler = handler);
            var eventBus = CreateEventBus(withTopicSubscription: true);
            await eventBus.SubscribeAsync<AccountRegisteredEvent, FirstHandler>();
            await eventBus.SubscribeAsync<AccountRegisteredEvent, SecondHandler>();
            await eventBus.RegisterSubscriptionListenerAsync();

            var @event = new AccountRegisteredEvent("user-1", "user@example.com");
            await messageHandler(CreateReceivedMessage(@event), CancellationToken.None);

            Assert.Equal(new[] { nameof(FirstHandler), nameof(SecondHandler) }, _handledEvents.OrderBy(x => x).ToArray());
            _subscriptionClient.Verify(x => x.CompleteAsync(It.IsAny<string>()), Times.Once);
        }

        private EventBusService CreateEventBus(bool withTopicSubscription)
        {
            var connection = new Mock<IServiceBusPersisterConnection>();
            if (withTopicSubscription)
            {
                connection.SetupGet(x => x.TopicConnection).Returns(CreateTopicConnection(_subscriptionClient.Object));
            }

            var rules = new List<RuleDescription>();
            _subscriptionClient.SetupGet(x => x.IsClosedOrClosing).Returns(false);
            _subscriptionClient.Setup(x => x.GetRulesAsync()).ReturnsAsync(() => rules.ToList());
            _subscriptionClient.Setup(x => x.AddRuleAsync(It.IsAny<RuleDescription>()))
                .Callback<RuleDescription>(rules.Add)
                .Returns(Task.CompletedTask);
            _subscriptionClient.Setup(x => x.RemoveRuleAsync(It.IsAny<string>()))
                .Callback<string>(name => rules.RemoveAll(rule => rule.Name == name))
                .Returns(Task.CompletedTask);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(_handledEvents)
                .AddTransient<FirstHandler>()
                .AddTransient<SecondHandler>()
                .AddTransient<DynamicHandler>()
                .BuildServiceProvider();

            return new EventBusService(
                connection.Object,
                _subscriptionsManager,
                serviceProvider,
                NullLoggerFactory.Instance,
                new MessageQueueConfiguration(),
                new EventPublishConfiguration("tests") { SendCompletionEvent = false });
        }

        private static ServiceBusTopicConnection CreateTopicConnection(ISubscriptionClient subscriptionClient)
        {
            // ServiceBusTopicConnection creates its clients lazily, so no connection is opened here.
            var topicConnection = new ServiceBusTopicConnection(
                "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
                new ServiceBusPersisterTopicConnectionConfiguration { TopicName = "topic", SubscriptionName = "subscription" });
            typeof(ServiceBusTopicConnection)
                .GetField("_subscriptionClient", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(topicConnection, subscriptionClient);
            return topicConnection;
        }

        // Marks the message as received so that its lock token can be completed.
        private static Message CreateReceivedMessage(AccountRegisteredEvent @event)
        {
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event))) { Label = EventName };
            var systemProperties = message.SystemProperties;
            var systemPropertiesType = systemProperties.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            systemPropertiesType.GetProperty("SequenceNumber", flags).SetValue(systemProperties, 1L);
            systemPropertiesType.GetProperty("LockTokenGuid", flags).SetValue(systemProperties, Guid.NewGuid());
            return message;
        }

        public class HandledEvents : ConcurrentBag<string>
        {
        }

        public class FirstHandler : IEventHandler<AccountRegisteredEvent>
        {
            private readonly HandledEvents _handledEvents;

            public FirstHandler(HandledEvents handledEvents)
            {
                _handledEvents = handledEvents;
            }

            public Task Handle(AccountRegisteredEvent @event)
            {
                _handledEvents.Add(nameof(FirstHandler));
                return Task.CompletedTask;
            }
        }

        public class SecondHandler : IEventHandler<AccountRegisteredEvent>
        {
            private readonly HandledEvents _handledEvents;

            public SecondHandler(HandledEvents handledEvents)
            {
                _handledEvents = handledEvents;
            }

            public Task Handle(AccountRegisteredEvent @event)
            {
                _handledEvents.Add(nameof(SecondHandler));
                return Task.CompletedTask;
            }
        }

        public class DynamicHandler : IDynamicEventHandler
        {
            public Task Handle(dynamic eventData)
            {
                return Task.CompletedTask;
            }
        }
    }
}
