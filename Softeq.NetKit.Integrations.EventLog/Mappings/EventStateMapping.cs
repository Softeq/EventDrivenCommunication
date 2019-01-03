// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Mappings.Abstract;

namespace Softeq.NetKit.Integrations.EventLog.Mappings
{
	internal class EventStateMapping : DomainModelBuilder<EventState>, IEntityMappingConfiguration
	{
		public override void Build(EntityTypeBuilder<EventState> builder)
		{
			builder.Property<int>(eventState => eventState.Id).ValueGeneratedNever();
		}
	}
}
