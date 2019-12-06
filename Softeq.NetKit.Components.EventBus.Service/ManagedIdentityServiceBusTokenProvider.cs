// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Azure.Services.AppAuthentication;

namespace Softeq.NetKit.Components.EventBus.Service
{
    public class ManagedIdentityServiceBusTokenProvider : TokenProvider
    {
        private readonly string _managedIdentityTenantId;

        public ManagedIdentityServiceBusTokenProvider(string managedIdentityTenantId = null)
        {
            _managedIdentityTenantId = string.IsNullOrEmpty(managedIdentityTenantId)
                ? null
                : managedIdentityTenantId;
        }

        public override async Task<SecurityToken> GetTokenAsync(string appliesTo, TimeSpan timeout)
        {
            var accessToken = await GetAccessToken("https://servicebus.azure.net/");
            return new JsonSecurityToken(accessToken, appliesTo);
        }

        private Task<string> GetAccessToken(string resource)
        {
            var authProvider = new AzureServiceTokenProvider();
            return authProvider.GetAccessTokenAsync(resource, _managedIdentityTenantId);
        }
    }
}