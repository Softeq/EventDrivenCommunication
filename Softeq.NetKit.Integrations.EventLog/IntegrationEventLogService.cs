// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Exceptions;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class IntegrationEventLogService : IIntegrationEventLogService
    {
        protected IntegrationEventLogContext EventLogContext; 
        public IntegrationEventLogService(IntegrationEventLogContext eventLogContext)
        {
            EventLogContext = eventLogContext;
        }

	    public async Task<IntegrationEventLog> GetAsync(Guid eventId)
	    {
		    var @event = await EventLogContext.IntegrationEventLogs.SingleOrDefaultAsync(el => el.EventId == eventId);
            if (@event == null)
            {
                throw new EventLogNotFoundException(eventId);
            }

            return @event;
        }

		public async Task SaveAsync(IntegrationEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            var eventLog = new IntegrationEventLog(@event);
            EventLogContext.IntegrationEventLogs.Add(eventLog);
            await EventLogContext.SaveChangesAsync();
        }

        public async Task MarkAsPublishedAsync(IntegrationEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            var eventLog = await EventLogContext.IntegrationEventLogs.SingleOrDefaultAsync(x => x.EventId == @event.Id);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(@event.Id);
            }

            eventLog.TimesSent++;
            eventLog.StateId = (int) EventStateEnum.Published;
            EventLogContext.IntegrationEventLogs.Update(eventLog);
            await EventLogContext.SaveChangesAsync();
        }

	    public async Task CompleteAsync(Guid eventId)
	    {
		    var eventLog = await EventLogContext.IntegrationEventLogs.SingleOrDefaultAsync(x => x.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }

            eventLog.StateId = (int)EventStateEnum.Completed;
            EventLogContext.IntegrationEventLogs.Update(eventLog);
            await EventLogContext.SaveChangesAsync();
        }

		public async Task UpdateAsync(IntegrationEventLog @event)
		{
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            @event.UpdatedTime = DateTimeOffset.UtcNow;
			EventLogContext.Update(@event);
			await EventLogContext.SaveChangesAsync();
		}
	}
}
