// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Utility;

namespace Softeq.NetKit.Integrations.EventLog.Mappings
{
    internal class IntegrationEventLogMappingConfiguration : DomainModelBuilder<IntegrationEventLog>, IEntityMappingConfiguration
    {
        private static readonly JsonSerializerSettings EventSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ContractResolver = new PrivateFieldContractResolver()
        };

        public override void Build(EntityTypeBuilder<IntegrationEventLog> builder)
        {
            builder.HasKey(eventLog => eventLog.EventLogId);
            builder.Property(eventLog => eventLog.EventState).HasConversion<int>().IsRequired();
            builder.HasIndex(eventLog => eventLog.EventState).IsUnique(false);
            builder.Property(eventLog => eventLog.TimesSent).IsRequired();
            builder.OwnsOne(entity => entity.EventEnvelope, ownershipBuilder =>
            {
                ownershipBuilder.HasIndex(x => x.Id).IsUnique();
                ownershipBuilder.Property(x => x.Created).IsRequired();
                ownershipBuilder.HasIndex(x => x.Created).IsUnique(false);
                ownershipBuilder.Property(x => x.PublisherId).IsRequired();
                ownershipBuilder.HasIndex(x => x.PublisherId).IsUnique(false);
                ownershipBuilder.Property(x => x.SequenceId).IsRequired(false);
                ownershipBuilder.HasIndex(x => x.SequenceId).IsUnique(false);
                ownershipBuilder.Property(x => x.CorrelationId).IsRequired(false);
                ownershipBuilder.Property(x => x.Event)
                    .HasConversion(
                        @event => JsonConvert.SerializeObject(@event, EventSerializerSettings),
                        json => (IntegrationEvent)JsonConvert.DeserializeObject(json, EventSerializerSettings))
                    .IsRequired();
            });
        }
    }
}