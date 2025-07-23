// Developed by Softeq Development Corporation
// http://www.softeq.com

using EnsureThat;

namespace Softeq.NetKit.Components.EventBus
{
    public class SubscriptionListenerConfiguration
    {
        public SubscriptionListenerConfiguration(int maxConcurrentCalls)
        {
            MaxConcurrentCalls = Ensure.Comparable.IsGt(maxConcurrentCalls, 0, nameof(maxConcurrentCalls));
        }

        public int MaxConcurrentCalls { get; }
    }
}