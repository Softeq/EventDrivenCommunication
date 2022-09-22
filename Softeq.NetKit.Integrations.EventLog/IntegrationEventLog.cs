// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Linq;
using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class IntegrationEventLog
    {
        private IntegrationEventLog()
        {
        }

        public IntegrationEventLog(IntegrationEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            EventId = @event.Id;
            Created = @event.CreationDate;
            EventTypeName = @event.GetType().FullName;
            EventState = EventState.NotPublished;
            SessionId = @event.SessionId;
            Content = @event;
        }

        public Guid EventId { get; private set; }
        public string EventTypeName { get; private set; }
        public EventState EventState { get; private set; }
        public int TimesSent { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset? Updated { get; set; }
        public string SessionId { get; private set; }
        public IntegrationEvent Content { get; private set; }

        public void ChangeEventState(EventState newEventState)
        {
            switch (newEventState)
            {
                case EventState.Published:
                    EnsureStateTransitionAllowed(EventState.NotPublished, EventState.PublishedFailed);
                    TimesSent++;
                    break;
                case EventState.PublishedFailed:
                    EnsureStateTransitionAllowed(EventState.Published);
                    break;
                case EventState.Completed:
                    EnsureStateTransitionAllowed(EventState.Published, EventState.PublishedFailed);
                    break;
                case EventState.NotPublished:
                default:
                    EnsureStateTransitionAllowed();
                    break;
            }

            EventState = newEventState;

            void EnsureStateTransitionAllowed(params EventState[] allowedStates)
            {
                if (!allowedStates.Any())
                {
                    throw new InvalidOperationException(
                        $"Unable to change event log state from '{EventState}' to '{newEventState}'.");
                }

                if (!allowedStates.Contains(EventState))
                {
                    throw new InvalidOperationException(
                        $"Unable to change event log state from '{EventState}' to '{newEventState}' " +
                        $"Allowed states: {string.Join(", ", allowedStates.Select(state => $"'{state}'"))}.");
                }
            }
        }
    }
}
