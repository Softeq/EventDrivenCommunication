// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Mappings.Abstract;

namespace Softeq.NetKit.Integrations.EventLog.Mappings
{
	internal class IntegrationEventLogMapping : DomainModelBuilder<IntegrationEventLog>, IEntityMappingConfiguration
	{
		public override void Build(EntityTypeBuilder<IntegrationEventLog> builder)
		{
			builder.HasKey(eventLog => eventLog.EventId);
			builder.Property(eventLog => eventLog.EventId).IsRequired();
			builder.Property(eventLog => eventLog.CreationTime).IsRequired();
			builder.Property(eventLog => eventLog.Content).IsRequired();
			builder.Property(eventLog => eventLog.StateId).IsRequired();
			builder.Property(eventLog => eventLog.EventTypeName).IsRequired();
			builder.Property(eventLog => eventLog.TimesSent).IsRequired();
		}
	}
}
