// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus
{
    public class EventSubscriptionInfo
    {
        private EventSubscriptionInfo(bool isDynamic, Type handlerType)
        {
            IsDynamic = isDynamic;
            HandlerType = handlerType;
        }

        public bool IsDynamic { get; }
        public Type HandlerType { get; }

        public static EventSubscriptionInfo Dynamic(Type handlerType)
        {
            return new EventSubscriptionInfo(true, handlerType);
        }

        public static EventSubscriptionInfo Typed(Type handlerType)
        {
            return new EventSubscriptionInfo(false, handlerType);
        }
    }
}