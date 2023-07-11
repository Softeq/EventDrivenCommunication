// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Components.EventBus.Service
{
    public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
    {
        Task HandleAsync(TEvent @event);
    }
}