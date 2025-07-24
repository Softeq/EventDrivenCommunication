// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Components.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class IntegrationEventLog
    {
        private IntegrationEventLog(string sessionId)
        {
            SessionId = sessionId;
        }

        public IntegrationEventLog(IntegrationEvent @event)
        {
            Ensure.Any.IsNotNull(@event, nameof(@event));

            EventId = @event.Id;
            EventTypeName = @event.GetType().FullName;
            EventState = EventState.NotPublished;
            Created = @event.CreationDate;
            SessionId = @event.SessionId;
            Content = @event;
        }

        public Guid EventId { get; private set; }
        public string EventTypeName { get; private set; }
        public EventState EventState { get; private set; }
        public int TimesSent { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset? Updated { get; private set; }
        public string SessionId { get; private set; }
        public IntegrationEvent Content { get; private set; }

        public void MarkAsPublished(string publisherId)
        {
            const EventState newEventState = EventState.Published;

            if (EventState == newEventState)
            {
                return;
            }

            EnsureStateTransitionAllowed(newEventState);

            EventState = newEventState;
            Updated = DateTimeOffset.UtcNow;
            Content.PublisherId = publisherId;
            TimesSent++;
        }

        public void MarkAsPublishAcknowledgmentTimeout()
        {
            const EventState newEventState = EventState.PublishAcknowledgmentTimeout;

            if (EventState == newEventState)
            {
                return;
            }

            EnsureStateTransitionAllowed(newEventState);

            EventState = newEventState;
            Updated = DateTimeOffset.UtcNow;
        }

        public void MarkAsCompleted()
        {
            const EventState newEventState = EventState.Completed;

            if (EventState == newEventState)
            {
                return;
            }

            EnsureStateTransitionAllowed(newEventState);

            EventState = newEventState;
            Updated = DateTimeOffset.UtcNow;
        }

        private void EnsureStateTransitionAllowed(EventState newEventState)
        {
            HashSet<EventState> transitionAllowedFrom;

            switch (newEventState)
            {
                case EventState.Published:
                    transitionAllowedFrom = new HashSet<EventState>
                    {
                        EventState.NotPublished,
                        EventState.PublishAcknowledgmentTimeout
                    };
                    break;
                case EventState.PublishAcknowledgmentTimeout:
                    transitionAllowedFrom = new HashSet<EventState>
                    {
                        EventState.Published
                    };
                    break;
                case EventState.Completed:
                    transitionAllowedFrom = new HashSet<EventState>
                    {
                        EventState.Published,
                        EventState.PublishAcknowledgmentTimeout
                    };
                    break;
                case EventState.NotPublished:
                default:
                    throw new InvalidOperationException($"Changing event log state to '{newEventState}' is not allowed.");
            }

            if (!transitionAllowedFrom.Contains(EventState))
            {
                var transitionAllowedFromString = string.Join(", ", transitionAllowedFrom.Select(state => $"'{state}'"));
                throw new InvalidOperationException(
                    $"Unable to change event log state from '{EventState}' to '{newEventState}'. " +
                    $"To make the transition, event should be in one of the following states: {transitionAllowedFromString}.");
            }
        }
    }
}
