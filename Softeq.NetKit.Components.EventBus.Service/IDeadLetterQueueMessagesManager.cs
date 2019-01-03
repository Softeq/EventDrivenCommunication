// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.NetKit.Components.EventBus.Service.Models;

namespace Softeq.NetKit.Components.EventBus.Service
{
    public interface IDeadLetterQueueMessagesManager
    {
        Task<List<DeadLetterQueueMessage>> GetSubscriptionMessages(string topicName, string subscriptionName);
        Task<List<DeadLetterQueueMessage>> GetQueueMessages(string queueName);
        Task RepublishTopicMessage(DeadLetterQueueMessage model);
        Task RepublishQueueMessage(DeadLetterQueueMessage model);
    }
}