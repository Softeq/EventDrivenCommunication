// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Components.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Softeq.NetKit.Integrations.EventLog.Dtos;

namespace Softeq.NetKit.Integrations.EventLog.Abstract
{
    public interface IIntegrationEventLogService
    {
        /// <summary>
        /// Returns an event log entry by its ID.
        /// </summary>
        /// <param name="eventId">The ID of event to be returned.</param>
        Task<IntegrationEventLog> GetAsync(Guid eventId);

        /// <summary>
        /// Gets a list of event logs that match the specified condition.
        /// </summary>
        /// <param name="condition">Match condition.</param>
        Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> condition);

        /// <summary>
        /// Returns a list of raw event contents filtered by the specified event states.
        /// It's important to return raw contents to avoid deserialization issues caused by event type changes.
        /// </summary>
        /// <param name="eventStates">Event states.</param>
        /// <param name="createdDateOrder">Creation date order.</param>
        /// <param name="takeCount">Max results count.</param>
        /// <returns>List of raw event contents.</returns>
        Task<List<IntegrationEventContentDto>> GetEventContentListAsync(
            List<EventState> eventStates,
            SortOrder createdDateOrder = SortOrder.Ascending,
            int takeCount = 100);

        /// <summary>
        /// Returns a boolean value indicating whether an event log matching the specified conditions exists.
        /// </summary>
        /// <param name="condition">Match condition.</param>
        Task<bool> AnyAsync(Expression<Func<IntegrationEventLog, bool>> condition);

        /// <summary>
        /// Creates a new event log entry for the specified integration event.
        /// </summary>
        /// <param name="event">Integration event to log.</param>
        Task<IntegrationEventLog> CreateAsync(IntegrationEvent @event);

        /// <summary>
        /// Marks the existing event log as published.
        /// </summary>
        /// <param name="eventId">The ID of event to be marked.</param>
        /// <param name="publisherId">Publisher ID.</param>
        Task<IntegrationEventLog> MarkAsPublishedAsync(Guid eventId, string publisherId);

        /// <summary>
        /// Marks the published event log as acknowledgment timeout received.
        /// </summary>
        /// <param name="eventId">The ID of event to be marked.</param>
        Task<IntegrationEventLog> MarkAsPublishAcknowledgmentTimeoutAsync(Guid eventId);

        /// <summary>
        /// Marks the published event log as completed.
        /// </summary>
        /// <param name="eventId">The ID of event to be marked.</param>
        Task<IntegrationEventLog> MarkAsCompletedAsync(Guid eventId);

        /// <summary>
        /// Deletes event logs by their IDs.
        /// </summary>
        /// <param name="eventIds">IDs of events to delete.</param>
        Task DeleteAsync(List<Guid> eventIds);
    }
}