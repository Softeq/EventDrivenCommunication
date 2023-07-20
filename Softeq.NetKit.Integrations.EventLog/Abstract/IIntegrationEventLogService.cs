// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Softeq.NetKit.Integrations.EventLog.Abstract
{
    public interface IIntegrationEventLogService
    {
        Task<IntegrationEventLog> GetAsync(Guid eventEnvelopeId);
        Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> condition);
        Task<bool> AnyAsync(Expression<Func<IntegrationEventLog, bool>> condition);
        Task CreateAsync(IntegrationEventLog eventLog);
        Task MarkAsPublishedAsync(IntegrationEventLog eventLog);
        Task MarkAsPublishedFailedAsync(IntegrationEventLog eventLog);
        Task MarkAsCompletedAsync(IntegrationEventLog eventLog);
    }
}