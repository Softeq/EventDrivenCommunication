// Developed by Softeq Development Corporation
// http://www.softeq.com

using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Softeq.NetKit.Integrations.EventLog.Extensions;
using Softeq.NetKit.Integrations.EventLog.Mappings;

namespace Softeq.NetKit.Integrations.EventLog
{
    public abstract class IntegrationEventLogContextBase<TContext> : DbContext
        where TContext : DbContext
    {
        private readonly IOptions<IntegrationEventLogContextOptions> _integrationEventLogOptions;

        protected IntegrationEventLogContextBase(
            DbContextOptions<TContext> options,
            IOptions<IntegrationEventLogContextOptions> integrationEventLogOptions)
            : base(options)
        {
            _integrationEventLogOptions = Ensure.Any.IsNotNull(integrationEventLogOptions, nameof(integrationEventLogOptions));
            Ensure.Any.IsNotNull(_integrationEventLogOptions.Value, $"{nameof(integrationEventLogOptions)}.{nameof(integrationEventLogOptions.Value)}");
            Ensure.String.IsNotNullOrEmpty(_integrationEventLogOptions.Value.Schema, nameof(IntegrationEventLogContextOptions.Schema));
            Ensure.String.IsNotNullOrEmpty(_integrationEventLogOptions.Value.TableName, nameof(IntegrationEventLogContextOptions.TableName));
        }

        public DbSet<IntegrationEventLog> IntegrationEventLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(_integrationEventLogOptions.Value.Schema);
            builder.AddEntityConfigurationsFromAssembly<IEntityMappingConfiguration>(GetType().Assembly);
            builder.Entity<IntegrationEventLog>().ToTable(_integrationEventLogOptions.Value.TableName, _integrationEventLogOptions.Value.Schema);
        }
    }
}
