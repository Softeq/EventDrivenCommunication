// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Softeq.NetKit.Components.EventBus.Abstract;
using Softeq.NetKit.Components.EventBus.Service.Tests.Samples.Events;

namespace Softeq.NetKit.Components.EventBus.Service.Tests.Samples.Handlers
{
    public class AccountRegisteredAuditHandler : IEventHandler<AccountRegisteredEvent>
    {
        public Task Handle(AccountRegisteredEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
