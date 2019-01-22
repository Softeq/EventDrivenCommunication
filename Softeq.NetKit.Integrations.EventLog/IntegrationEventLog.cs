// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Newtonsoft.Json;
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
            EventId = @event.Id;
            CreationTime = @event.CreationDate;
            EventTypeName = @event.GetType().FullName;
            StateId = (int)EventStateEnum.NotPublished;

            SetContent(@event);
        }

        public Guid EventId { get; set; }
        public string EventTypeName { get; set; }
        public int StateId { get; set; }
        public virtual EventState State { get; set; }
        public int TimesSent { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset? UpdatedTime { get; set; }
        public string Content { get; set; }

        public void SetContent(IntegrationEvent @event)
        {
            Content = JsonConvert.SerializeObject(@event);
        }
    }
}
