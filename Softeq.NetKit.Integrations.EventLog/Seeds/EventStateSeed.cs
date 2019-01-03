// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Seeds.Abstract;

namespace Softeq.NetKit.Integrations.EventLog.Seeds
{
	internal class EventStateSeed : DomainModelBuilder<EventState>, IEntitySeedConfiguration
	{
		public override void Build(EntityTypeBuilder<EventState> builder)
		{
			builder.HasData(
				new EventState {Id = (int) EventStateEnum.Published, Name = nameof(EventStateEnum.Published) },
				new EventState { Id = (int) EventStateEnum.Completed, Name = nameof(EventStateEnum.Completed) },
				new EventState { Id = (int) EventStateEnum.PublishedFailed, Name = nameof(EventStateEnum.PublishedFailed) },
				new EventState { Id = (int) EventStateEnum.NotPublished, Name = nameof(EventStateEnum.NotPublished) });
		}
	}
}
