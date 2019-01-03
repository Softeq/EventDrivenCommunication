// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Components.EventBus.Service.Models
{
    public class DeadLetterQueueMessage
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public string JsonResponse { get; set; }
        public string EventName { get; set; }
    }
}
