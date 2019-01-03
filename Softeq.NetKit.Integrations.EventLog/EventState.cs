// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class EventState
    {
	    public EventState()
	    {
	    }

	    public EventState(EventStateEnum eventState)
	    {
		    Id = (int)eventState;
		    Name = eventState.ToString();
	    }

		public int Id { get; set; }
	    public string Name { get; set; }
	    public virtual ICollection<IntegrationEventLog> EventLogs { get; set; }

	    public static implicit operator EventStateEnum(EventState eventState) => (EventStateEnum)eventState.Id;
	    public static implicit operator EventState(EventStateEnum eventState) => new EventState(eventState);
    }
}
