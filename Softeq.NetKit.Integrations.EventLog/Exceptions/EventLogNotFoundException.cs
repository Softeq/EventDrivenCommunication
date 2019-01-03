using System;

namespace Softeq.NetKit.Integrations.EventLog.Exceptions
{
    public class EventLogNotFoundException : Exception
    {
        public EventLogNotFoundException(Guid id) : base($"Integration event log with Id {id} was not found")
        {
        }
    }
}
