// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;
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
            var eventLog = await EventLogContext.IntegrationEventLogs.FirstOrDefaultAsync(log => log.Content.Id == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }

            return eventLog;
        }

        public async Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            return await EventLogContext.IntegrationEventLogs.Where(condition).ToListAsync();
        }

        public Task CreateAsync(IntegrationEventLog eventLog)
        {
            EventLogContext.IntegrationEventLogs.Add(eventLog);
            return EventLogContext.SaveChangesAsync();
        }

        public async Task MarkAsPublishedAsync(IntegrationEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(nameof(eventLog));
            }

            eventLog.ChangeEventState(EventState.Published);
            await UpdateAsync(eventLog);
        }

        public async Task MarkAsPublishedFailedAsync(IntegrationEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(nameof(eventLog));
            }

            eventLog.ChangeEventState(EventState.PublishedFailed);
            await UpdateAsync(eventLog);
        }

        public async Task MarkAsCompletedAsync(IntegrationEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(nameof(eventLog));
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

            EventLogContext.IntegrationEventLogs.Update(@event);
            await EventLogContext.SaveChangesAsync();
        }
    }
}