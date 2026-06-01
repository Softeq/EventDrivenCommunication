// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class IntegrationEventLogContext : IntegrationEventLogContextBase<IntegrationEventLogContext>
    {
        public IntegrationEventLogContext(DbContextOptions<IntegrationEventLogContext> options)
            : base(options, "dbo", "IntegrationEventLogs")
        {
        }
    }
}
