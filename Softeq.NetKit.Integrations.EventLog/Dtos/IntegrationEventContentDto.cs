// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Integrations.EventLog.Dtos
{
    public class IntegrationEventContentDto
    {
        /// <summary>
        /// Identifier of the event.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// The full name of the event type.
        /// </summary>
        public string EventTypeName { get; set; }

        /// <summary>
        /// The JSON string representation of the event.
        /// </summary>
        public string EventJsonString { get; set; }
    }
}