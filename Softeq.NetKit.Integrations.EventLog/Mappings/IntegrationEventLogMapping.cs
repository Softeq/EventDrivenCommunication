// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Utility;

namespace Softeq.NetKit.Integrations.EventLog.Mappings
{
    internal class IntegrationEventLogMapping : DomainModelBuilder<IntegrationEventLog>, IEntityMappingConfiguration
    {
        public override void Build(EntityTypeBuilder<IntegrationEventLog> builder)
        {
            builder.HasKey(eventLog => eventLog.EventId);
            builder.Property(eventLog => eventLog.EventId).IsRequired();

            builder.Property(eventLog => eventLog.Created).IsRequired();
            builder.HasIndex(eventLog => eventLog.Created).IsUnique(false);

            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new PrivateFieldContractResolver()
            };
            // TODO: Get rid of EventLog properties serialization within Content object
            builder.Property(eventLog => eventLog.Content)
                .HasConversion(
                    integrationEvent => JsonConvert.SerializeObject(integrationEvent, serializerSettings),
                    json => (IntegrationEvent)JsonConvert.DeserializeObject(json, serializerSettings))
                .IsRequired();

            builder.Property(eventLog => eventLog.EventState).HasConversion<int>().IsRequired();
            builder.HasIndex(eventLog => eventLog.EventState).IsUnique(false);

            builder.Property(eventLog => eventLog.EventTypeName).IsRequired();

            builder.Property(eventLog => eventLog.TimesSent).IsRequired();

            builder.Property(eventLog => eventLog.SessionId).IsRequired(false);
            builder.HasIndex(eventLog => eventLog.SessionId).IsUnique(false);
        }
    }
}