// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Integrations.EventLog.Abstract
{
    public interface IIntegrationEventLogService
    {
        Task<IntegrationEventLog> GetAsync(Guid eventId);
        Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> where);
        Task SaveAsync(IntegrationEvent @event);
        Task MarkAsPublishedAsync(IntegrationEvent @event);
        Task CompleteAsync(Guid eventId);
        Task UpdateAsync(IntegrationEventLog @event);
    }
}
