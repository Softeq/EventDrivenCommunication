// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.Azure.ServiceBus.Primitives;

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public class ServiceBusMsiConnectionConfiguration : ServiceBusPersisterConnectionConfiguration
    {
        public string NamespaceName { get; set; }
        public TokenProvider TokenProvider { get; set; } = null;
    }
}