// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus
{
    // TODO: Is that needed?
    public class EventSubscriptionInfo
    {
        public EventSubscriptionInfo(Type handlerType)
        {
            HandlerType = handlerType;
        }

        public Type HandlerType { get; }
    }
}