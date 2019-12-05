// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.NetKit.Components.EventBus
{
    public class EventPublishConfiguration
    {
        public EventPublishConfiguration(string eventPublisherId)
        {
            EventPublisherId = eventPublisherId;
            SendCompletionEvent = true;
        }

        public string EventPublisherId { get; }
        public bool SendCompletionEvent { get; set; }
    }
}
