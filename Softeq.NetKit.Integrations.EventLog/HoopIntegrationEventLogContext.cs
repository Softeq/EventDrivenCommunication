// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class HoopIntegrationEventLogContext : IntegrationEventLogContextBase<HoopIntegrationEventLogContext>
    {
        public HoopIntegrationEventLogContext(
            DbContextOptions<HoopIntegrationEventLogContext> dbContextOptions,
            IOptions<IntegrationEventLogContextOptions> integrationEventLogContextOptions)
            : base(dbContextOptions, integrationEventLogContextOptions)
        {
        }
    }
}
