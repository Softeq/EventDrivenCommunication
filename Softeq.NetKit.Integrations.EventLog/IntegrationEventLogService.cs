// Developed by Softeq Development Corporation
// http://www.softeq.com

using EnsureThat;
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
        private readonly Func<IntegrationEventLogContext> _dbContextFactory;

        protected IntegrationEventLogContext DbContext => _dbContextFactory.Invoke();

        public IntegrationEventLogService(Func<IntegrationEventLogContext> dbContextFactory)
        {
            _dbContextFactory = Ensure.Any.IsNotNull(dbContextFactory, nameof(dbContextFactory));
        }

        public async Task<IntegrationEventLog> GetAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await DbContext
                .IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            return eventLog;
        }

        public Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> condition)
        {
            Ensure.Any.IsNotNull(condition, nameof(condition));

            return DbContext.IntegrationEventLogs.Where(condition).ToListAsync();
        }

        public async Task<List<IntegrationEventLog>> GetAsync(
            Expression<Func<IntegrationEventLog, bool>> condition,
            Expression<Func<IntegrationEventLog, object>> orderByProperty,
            SortOrder sortOrder = SortOrder.Ascending,
            int takeCount = 100,
            bool asNoTracking = true)
        {
            Ensure.Any.IsNotNull(condition, nameof(condition));
            Ensure.Any.IsNotNull(orderByProperty, nameof(orderByProperty));
            Ensure.Comparable.IsGt(takeCount, 0, nameof(takeCount));

            IQueryable<IntegrationEventLog> query = DbContext.IntegrationEventLogs;

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            query = query.Where(condition);

            query = sortOrder == SortOrder.Ascending
                ? query.OrderBy(orderByProperty)
                : query.OrderByDescending(orderByProperty);

            if (takeCount > 0)
            {
                query = query.Take(takeCount);
            }

            return await query.ToListAsync();
        }

        public Task<bool> AnyAsync(Expression<Func<IntegrationEventLog, bool>> condition)
        {
            Ensure.Any.IsNotNull(condition, nameof(condition));

            return DbContext.IntegrationEventLogs.AnyAsync(condition);
        }

        public async Task<IntegrationEventLog> CreateAsync(IntegrationEvent @event)
        {
            Ensure.Any.IsNotNull(@event, nameof(@event));

            var eventLog = new IntegrationEventLog(@event);
            DbContext.IntegrationEventLogs.Add(eventLog);
            await DbContext.SaveChangesAsync();
            return eventLog;
        }

        public async Task<IntegrationEventLog> MarkAsPublishedAsync(Guid eventId, string publisherId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));
            Ensure.String.IsNotNullOrEmpty(publisherId, nameof(publisherId));

            var eventLog = await DbContext
                .IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            eventLog.MarkAsPublished(publisherId);
            await UpdateAsync(eventLog);
            return eventLog;
        }

        public async Task<IntegrationEventLog> MarkAsPublishAcknowledgmentTimeoutAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await DbContext
                .IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            eventLog.MarkAsPublishAcknowledgmentTimeout();
            await UpdateAsync(eventLog);
            return eventLog;
        }

        public async Task<IntegrationEventLog> MarkAsCompletedAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await DbContext
                .IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            eventLog.MarkAsCompleted();
            await UpdateAsync(eventLog);
            return eventLog;
        }

        public async Task DeleteAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await DbContext.IntegrationEventLogs.FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }

            DbContext.IntegrationEventLogs.Remove(eventLog);
            await DbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Expression<Func<IntegrationEventLog, bool>> condition)
        {
            Ensure.Any.IsNotNull(condition, nameof(condition));

            var eventLogsToDelete = await DbContext.IntegrationEventLogs.Where(condition).ToListAsync();
            DbContext.IntegrationEventLogs.RemoveRange(eventLogsToDelete);
            await DbContext.SaveChangesAsync();
        }

        private async Task UpdateAsync(IntegrationEventLog @event)
        {
            DbContext.IntegrationEventLogs.Update(@event);
            await DbContext.SaveChangesAsync();
        }
    }
}