// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Components.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Softeq.NetKit.Integrations.EventLog.Abstract
{
    public interface IIntegrationEventLogService
    {
        Task<IntegrationEventLog> GetAsync(Guid eventId);
        Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> condition);
        Task<List<IntegrationEventLog>> GetAsync(
            Expression<Func<IntegrationEventLog, bool>> condition,
            Expression<Func<IntegrationEventLog, object>> orderByProperty,
            SortOrder sortOrder = SortOrder.Ascending,
            int takeCount = 100,
            bool asNoTracking = true);
        Task<bool> AnyAsync(Expression<Func<IntegrationEventLog, bool>> condition);
        Task<IntegrationEventLog> CreateAsync(IntegrationEvent @event);
        Task<IntegrationEventLog> MarkAsPublishedAsync(Guid eventId, string publisherId);
        Task<IntegrationEventLog> MarkAsPublishAcknowledgmentTimeoutAsync(Guid eventId);
        Task<IntegrationEventLog> MarkAsCompletedAsync(Guid eventId);
        Task DeleteAsync(Guid eventId);
        Task DeleteAsync(Expression<Func<IntegrationEventLog, bool>> condition);
    }
}