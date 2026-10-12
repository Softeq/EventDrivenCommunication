// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Linq;
using Softeq.NetKit.Components.EventBus.Managers;
using Softeq.NetKit.Components.EventBus.Service.Tests.Samples.Events;
using Softeq.NetKit.Components.EventBus.Service.Tests.Samples.Handlers;
using Xunit;

namespace Softeq.NetKit.Components.EventBus.Service.Tests
{
    public class EventBusSubscriptionsManagerTests
    {
        private const string EventName = nameof(AccountRegisteredEvent);

        [Fact]
        public void GetEventTypeByName_TwoHandlersForSameEvent_ReturnsEventType()
        {
            var manager = new EventBusSubscriptionsManager();
            manager.AddSubscription<AccountRegisteredEvent, AccountRegisteredEventHandler>();
            manager.AddSubscription<AccountRegisteredEvent, AccountRegisteredAuditHandler>();

            Assert.Equal(typeof(AccountRegisteredEvent), manager.GetEventTypeByName(EventName));
            Assert.Equal(2, manager.GetEventHandlers(EventName).Count());
        }

        [Fact]
        public void RemoveSubscription_OneOfTwoHandlers_EventTypeStillResolvable()
        {
            var manager = new EventBusSubscriptionsManager();
            manager.AddSubscription<AccountRegisteredEvent, AccountRegisteredEventHandler>();
            manager.AddSubscription<AccountRegisteredEvent, AccountRegisteredAuditHandler>();

            manager.RemoveSubscription<AccountRegisteredEvent, AccountRegisteredEventHandler>();

            Assert.Equal(typeof(AccountRegisteredEvent), manager.GetEventTypeByName(EventName));
            Assert.Equal(typeof(AccountRegisteredAuditHandler), manager.GetEventHandlers(EventName).Single().HandlerType);
        }

        [Fact]
        public void RemoveSubscription_LastHandler_RemovesEventType()
        {
            var manager = new EventBusSubscriptionsManager();
            string removedEvent = null;
            manager.OnEventRemoved += (sender, name) => removedEvent = name;
            manager.AddSubscription<AccountRegisteredEvent, AccountRegisteredEventHandler>();
            manager.AddSubscription<AccountRegisteredEvent, AccountRegisteredAuditHandler>();

            manager.RemoveSubscription<AccountRegisteredEvent, AccountRegisteredEventHandler>();
            Assert.Null(removedEvent);
            manager.RemoveSubscription<AccountRegisteredEvent, AccountRegisteredAuditHandler>();

            Assert.Equal(EventName, removedEvent);
            Assert.Null(manager.GetEventTypeByName(EventName));
            Assert.False(manager.HasSubscriptionsForEvent(EventName));
        }

        [Fact]
        public void AddSubscription_SameHandlerTwice_Throws()
        {
            var manager = new EventBusSubscriptionsManager();
            manager.AddSubscription<AccountRegisteredEvent, AccountRegisteredEventHandler>();

            Assert.Throws<System.ArgumentException>(() =>
                manager.AddSubscription<AccountRegisteredEvent, AccountRegisteredEventHandler>());
            Assert.Equal(typeof(AccountRegisteredEvent), manager.GetEventTypeByName(EventName));
        }
    }
}
