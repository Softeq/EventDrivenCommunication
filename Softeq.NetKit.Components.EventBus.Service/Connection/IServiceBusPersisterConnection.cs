// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Microsoft.Azure.ServiceBus.Core;

namespace Softeq.NetKit.Components.EventBus.Service.Connection
{
    public interface IServiceBusPersisterConnection : IAsyncDisposable
    {
        ServiceBusPersisterConnectionConfiguration ConnectionConfiguration { get; }
        ServiceBusTopicConnection TopicConnection { get; }
        ServiceBusQueueConnection QueueConnection { get; }
        MessageReceiver CreateMessageReceiver(string entity);
    }
}