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
        public IntegrationEventLogContext(DbContextOptions<IntegrationEventLogContext> options, string defaultSchema) : base(options)
        {
            _defaultSchema = defaultSchema;
        }

        public DbSet<IntegrationEventLog> IntegrationEventLogs { get; set; }

        private readonly string _defaultSchema;

        protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasDefaultSchema(_defaultSchema);
		    //put db configuration here
		    base.OnModelCreating(builder);

		    builder.AddEntityConfigurationsFromAssembly<IEntityMappingConfiguration>(GetType().Assembly);
		    builder.AddEntityConfigurationsFromAssembly<IEntitySeedConfiguration>(GetType().Assembly);
	    }
    }
}
