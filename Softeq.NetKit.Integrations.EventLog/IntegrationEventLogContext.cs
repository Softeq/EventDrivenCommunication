// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;
using Softeq.NetKit.Integrations.EventLog.Extensions;
using Softeq.NetKit.Integrations.EventLog.Mappings.Abstract;
using Softeq.NetKit.Integrations.EventLog.Seeds.Abstract;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class IntegrationEventLogContext : DbContext
    {
        public IntegrationEventLogContext(DbContextOptions<IntegrationEventLogContext> options) : base(options)
        {
        }

        public DbSet<IntegrationEventLog> IntegrationEventLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
		{
		    //put db configuration here
		    base.OnModelCreating(builder);

		    builder.AddEntityConfigurationsFromAssembly<IEntityMappingConfiguration>(GetType().Assembly);
		    builder.AddEntityConfigurationsFromAssembly<IEntitySeedConfiguration>(GetType().Assembly);
	    }
    }
}
