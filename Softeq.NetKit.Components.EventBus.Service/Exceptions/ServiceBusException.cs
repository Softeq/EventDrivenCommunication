// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Runtime.Serialization;

namespace Softeq.NetKit.Components.EventBus.Service.Exceptions
{
    [Serializable]
    public class ServiceBusException : Exception
    {
        public ServiceBusException(string message)
            : base(message)
        {
        }

        public ServiceBusException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ServiceBusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
