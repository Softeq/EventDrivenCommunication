// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Softeq.NetKit.Components.EventBus.Service.Connection;
using Softeq.NetKit.Components.EventBus.Service.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Softeq.NetKit.Components.EventBus.Service
{
    public class DeadLetterQueueMessagesManager : IDeadLetterQueueMessagesManager
    {
        private const int MaxMessageCount = 1000;
        private readonly IServiceBusPersisterConnection _serviceBusPersisterConnection;

        public DeadLetterQueueMessagesManager(IServiceBusPersisterConnection serviceBusPersisterConnection)
        {
            _serviceBusPersisterConnection = serviceBusPersisterConnection;
        }

        public async Task<List<DeadLetterQueueMessage>> GetSubscriptionMessages(string topicName, string subscriptionName)
        {
            var deadLetterName = EntityNameHelper.FormatDeadLetterPath(EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName));
            var messages = await GetMessages(deadLetterName);
            return messages;
        }

        public async Task<List<DeadLetterQueueMessage>> GetQueueMessages(string queueName)
        {
            var deadLetterName = EntityNameHelper.FormatDeadLetterPath(queueName);
            var messages = await GetMessages(deadLetterName);
            return messages;
        }

        private async Task<List<DeadLetterQueueMessage>> GetMessages(string entityPath)
        {
            var dlqMessages = new List<DeadLetterQueueMessage>();
            var client = _serviceBusPersisterConnection.CreateMessageReceiver(entityPath);

            var queueStillHasMessages = true;
            while (queueStillHasMessages)
            {
                var tokens = new List<string>(MaxMessageCount);

                var messages = await client.ReceiveAsync(MaxMessageCount);
                if (messages == null || !messages.Any())
                {
                    queueStillHasMessages = false;
                    continue;
                }

                foreach (var msg in messages)
                {
                    using (var reader = new StreamReader(msg.GetBody<Stream>(), Encoding.UTF8))
                    {
                        var deadLetterQueueMessage = new DeadLetterQueueMessage
                        {
                            Id = msg.MessageId,
                            Created = msg.ScheduledEnqueueTimeUtc,
                            EventName = msg.Label,
                            JsonResponse = await reader.ReadToEndAsync()
                        };
                        dlqMessages.Add(deadLetterQueueMessage);
                    }

                    tokens.Add(msg.SystemProperties.LockToken);
                }
                await client.CompleteAsync(tokens);
            }
            return dlqMessages;
        }

        public Task RepublishTopicMessage(DeadLetterQueueMessage model)
        {
            return PushMessage(_serviceBusPersisterConnection.TopicConnection.TopicClient, model);
        }

        public Task RepublishQueueMessage(DeadLetterQueueMessage model)
        {
            return PushMessage(_serviceBusPersisterConnection.QueueConnection.QueueClient, model);
        }

        private static Task PushMessage(ISenderClient client, DeadLetterQueueMessage model)
        {
            var eventName = model.EventName;
            var body = Encoding.UTF8.GetBytes(model.JsonResponse);

            var message = new Message
            {
                MessageId = model.Id,
                Body = body,
                Label = eventName
            };

            return client.SendAsync(message);
        }
    }
}
