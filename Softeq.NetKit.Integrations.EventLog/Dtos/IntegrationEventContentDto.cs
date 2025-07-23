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

        /// <summary>
        /// The date and time when the entity was created.
        /// </summary>
        public DateTimeOffset Created { get; set; }
    }
}