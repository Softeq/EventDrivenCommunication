// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.NetKit.Components.EventBus
{
    public class EventPublishConfiguration
    {
        public EventPublishConfiguration(string eventPublisherId)
        {
            EventPublisherId = eventPublisherId;
        }

        public string EventPublisherId { get; }
    }
}
