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
            EventLogContext = eventLogContext ?? throw new ArgumentNullException(nameof(eventLogContext));
        }

        public Task CreateAsync(IntegrationEventLog eventLog)
        {
            EventLogContext.IntegrationEventLogs.Add(eventLog);
            return EventLogContext.SaveChangesAsync();
        }

        public async Task<IntegrationEventLog> GetAsync(Guid eventEnvelopeId)
        {
            var eventLog = await EventLogContext.IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventEnvelope.Id == eventEnvelopeId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventEnvelopeId);
            }

            return eventLog;
        }

        public Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            return EventLogContext.IntegrationEventLogs.Where(condition).ToListAsync();
        }

        public Task<bool> AnyAsync(Expression<Func<IntegrationEventLog, bool>> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            return EventLogContext.IntegrationEventLogs.AnyAsync(condition);
        }

        public Task MarkAsPublishedAsync(IntegrationEventLog eventLog)
        {
            return UpdateEventStateAsync(eventLog, EventState.Published);
        }

        public Task MarkAsPublishFailedAsync(IntegrationEventLog eventLog)
        {
            return UpdateEventStateAsync(eventLog, EventState.PublishFailed);
        }

        public Task MarkAsCompletedAsync(IntegrationEventLog eventLog)
        {
            return UpdateEventStateAsync(eventLog, EventState.Completed);
        }

        private Task UpdateEventStateAsync(IntegrationEventLog eventLog, EventState newState)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(nameof(eventLog));
            }

            eventLog.ChangeEventState(newState);
            EventLogContext.IntegrationEventLogs.Update(eventLog);
            return EventLogContext.SaveChangesAsync();
        }
    }
}