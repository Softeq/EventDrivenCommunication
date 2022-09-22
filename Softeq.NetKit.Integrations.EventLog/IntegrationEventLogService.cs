// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        public async Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> where)
        {
            return await EventLogContext.IntegrationEventLogs.Where(where).ToListAsync();
        }

        public Task CreateAsync(IntegrationEvent @event)
        {
            var eventLog = new IntegrationEventLog(@event);
            EventLogContext.IntegrationEventLogs.Add(eventLog);
            return EventLogContext.SaveChangesAsync();
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

            // Published event has PublisherId so need to update it also
            eventLog.Content.PublisherId = @event.PublisherId;
            eventLog.ChangeEventState(EventState.Published);

            await UpdateAsync(eventLog);
        }

        public async Task MarkAsPublishedFailedAsync(IntegrationEvent @event)
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

            eventLog.ChangeEventState(EventState.PublishedFailed);

            await UpdateAsync(eventLog);
        }

        public async Task MarkAsCompletedAsync(Guid eventId)
        {
            var eventLog = await EventLogContext.IntegrationEventLogs.SingleOrDefaultAsync(x => x.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            
            eventLog.ChangeEventState(EventState.Completed);

            await UpdateAsync(eventLog);
        }

        private async Task UpdateAsync(IntegrationEventLog @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            @event.Updated = DateTimeOffset.UtcNow;
            EventLogContext.IntegrationEventLogs.Update(@event);
            await EventLogContext.SaveChangesAsync();
        }
    }
}