namespace Softeq.NetKit.Components.EventBus
{
    public class QueueListenerConfiguration
    {
        public QueueListenerConfiguration()
        {
            MaxConcurrent = 10;
            UseSessions = false;
        }

        public int MaxConcurrent { get; set; }
        public bool UseSessions { get; set; }
    }
}