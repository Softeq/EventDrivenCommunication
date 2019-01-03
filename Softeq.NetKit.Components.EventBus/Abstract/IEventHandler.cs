// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Components.EventBus.Events;
using System.Threading.Tasks;

namespace Softeq.NetKit.Components.EventBus.Abstract
{
    public interface IEventHandler<in TIntegrationEvent> : IEventHandler where TIntegrationEvent: IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }

    public interface IEventHandler
    {
    }
}
