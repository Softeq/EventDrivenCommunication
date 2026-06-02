// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class IntegrationEventLogContext : IntegrationEventLogContextBase<IntegrationEventLogContext>
    {
        public IntegrationEventLogContext(
            DbContextOptions<IntegrationEventLogContext> options,
            IOptions<IntegrationEventLogContextOptions> integrationEventLogContextOptions)
            : base(options, integrationEventLogContextOptions)
        {
        }
    }
}
