// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus.Events
{
    public class CompletedEvent : IntegrationEvent
    {
	    public CompletedEvent(Guid completedEventId)
	    {
		    CompletedEventId = completedEventId;
	    }

	    public Guid CompletedEventId { get; }
	}
}
