// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;
using Softeq.NetKit.Integrations.EventLog.Extensions;
using Softeq.NetKit.Integrations.EventLog.Mappings;

namespace Softeq.NetKit.Integrations.EventLog
{
    public abstract class IntegrationEventLogContextBase<TContext> : DbContext
        where TContext : DbContext
    {
        private readonly string _schema;
        private readonly string _tableName;

        protected IntegrationEventLogContextBase(
            DbContextOptions<TContext> options,
            string schema,
            string tableName)
            : base(options)
        {
            _schema = schema;
            _tableName = tableName;
        }

        public DbSet<IntegrationEventLog> IntegrationEventLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(_schema);
            builder.AddEntityConfigurationsFromAssembly<IEntityMappingConfiguration>(GetType().Assembly);
            builder.Entity<IntegrationEventLog>().ToTable(_tableName, _schema);
        }
    }
}
