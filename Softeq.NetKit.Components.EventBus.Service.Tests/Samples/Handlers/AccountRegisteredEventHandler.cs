// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Softeq.NetKit.Components.EventBus.Abstract;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Components.EventBus.Service.Tests.Samples.Events;

namespace Softeq.NetKit.Components.EventBus.Service.Tests.Samples.Handlers
{
    public class AccountRegisteredEventHandler : IEventEnvelopeHandler<AccountRegisteredEvent>
    {
        // Inject your app service

        // TODO: Need to change this action according to requirements
        public Task HandleAsync(IntegrationEventEnvelope<AccountRegisteredEvent> eventEnvelope)
        {
            throw new System.NotImplementedException();
        }
    }
}
