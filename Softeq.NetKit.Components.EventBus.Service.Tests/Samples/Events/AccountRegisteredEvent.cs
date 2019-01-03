// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Components.EventBus.Events;

namespace Softeq.NetKit.Components.EventBus.Service.Tests.Samples.Events
{
    public class AccountRegisteredEvent : IntegrationEvent
    {
        public string UserId { get; }
        public string Email { get; }

        public AccountRegisteredEvent(string userId, string email)
        {
            UserId = userId;
            Email = email;
        }
    }
}
