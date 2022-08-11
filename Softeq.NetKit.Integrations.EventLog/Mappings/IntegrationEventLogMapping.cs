// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Mappings.Abstract;
using Softeq.NetKit.Integrations.EventLog.Utility;

namespace Softeq.NetKit.Integrations.EventLog.Mappings
{
    internal class IntegrationEventLogMapping : DomainModelBuilder<IntegrationEventLog>, IEntityMappingConfiguration
    {
        public override void Build(EntityTypeBuilder<IntegrationEventLog> builder)
        {
            builder.HasKey(eventLog => eventLog.EventId);
            builder.Property(eventLog => eventLog.EventId).IsRequired();
            builder.Property(eventLog => eventLog.CreationTime).IsRequired();

            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new PrivateFieldContractResolver()
            };

            builder.Property(eventLog => eventLog.Content)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, serializerSettings),
                    v => (IntegrationEvent)JsonConvert.DeserializeObject(v, serializerSettings))
                .IsRequired();
            builder.Property(eventLog => eventLog.StateId).IsRequired();
            builder.Property(eventLog => eventLog.EventTypeName).IsRequired();
            builder.Property(eventLog => eventLog.TimesSent).IsRequired();
            builder.Property(eventLog => eventLog.SessionId).IsRequired(false);
        }
    }
}
