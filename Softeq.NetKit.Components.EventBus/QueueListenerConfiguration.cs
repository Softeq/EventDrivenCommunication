// Developed by Softeq Development Corporation
// http://www.softeq.com

using EnsureThat;

namespace Softeq.NetKit.Components.EventBus
{
    public class QueueListenerConfiguration
    {
        public QueueListenerConfiguration(bool useSessions, int maxConcurrent)
        {
            UseSessions = useSessions;
            MaxConcurrent = Ensure.Comparable.IsGt(maxConcurrent, 0, nameof(maxConcurrent));
        }

        public bool UseSessions { get; }
        public int MaxConcurrent { get; }
    }
}