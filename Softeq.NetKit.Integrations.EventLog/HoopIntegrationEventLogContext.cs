// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class HoopIntegrationEventLogContext : IntegrationEventLogContextBase<HoopIntegrationEventLogContext>
    {
        public HoopIntegrationEventLogContext(DbContextOptions<HoopIntegrationEventLogContext> options)
            : base(options, "dbo", "HoopIntegrationEventLogs")
        {
        }
    }
}
