// Developed by Softeq Development Corporation
// http://www.softeq.com

using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Softeq.NetKit.Integrations.EventLog.Dtos;
using SortOrder = Softeq.NetKit.Integrations.EventLog.Dtos.SortOrder;

namespace Softeq.NetKit.Integrations.EventLog
{
    public class IntegrationEventLogService : IIntegrationEventLogService
    {
        private readonly Func<IntegrationEventLogContext> _dbContextFactory;

        protected IntegrationEventLogContext DbContext => _dbContextFactory.Invoke();

        public IntegrationEventLogService(Func<IntegrationEventLogContext> dbContextFactory)
        {
            _dbContextFactory = Ensure.Any.IsNotNull(dbContextFactory, nameof(dbContextFactory));
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> GetAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await DbContext
                .IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            return eventLog;
        }

        /// <inheritdoc />
        public Task<List<IntegrationEventLog>> GetAsync(Expression<Func<IntegrationEventLog, bool>> condition)
        {
            Ensure.Any.IsNotNull(condition, nameof(condition));

            return DbContext.IntegrationEventLogs.Where(condition).ToListAsync();
        }

        /// <inheritdoc />
        public Task<List<IntegrationEventContentDto>> GetEventContentListAsync(
            List<EventState> eventStates,
            SortOrder createdDateOrder = SortOrder.Ascending,
            int takeCount = 100)
        {
            Ensure.Any.IsNotNull(eventStates, nameof(eventStates));
            Ensure.Collection.HasItems(eventStates, nameof(eventStates));
            Ensure.Comparable.IsGt(takeCount, 0, nameof(takeCount));

            var stateParamNames = eventStates.Select((_, i) => $"@state{i}").ToList();
            var eventStateInClause = string.Join(", ", stateParamNames);
            var orderClause = createdDateOrder == SortOrder.Ascending ? "ASC" : "DESC";

            var sql = $@"
                SELECT TOP (@takeCount) [EventId], [EventTypeName], [Content]
                FROM [IntegrationEventLogs]
                WHERE [EventState] IN ({eventStateInClause})
                ORDER BY [Created] {orderClause}";

            var parameters = new List<DbParameter>
            {
                new SqlParameter("@takeCount", SqlDbType.Int) { Value = takeCount }
            };
            parameters
                .AddRange(eventStates
                    .Select((state, i) => new SqlParameter($"@state{i}", SqlDbType.Int)
                    {
                        Value = (int)state
                    }));

            return ExecuteWithConnectionAsync(sql, parameters, async command =>
            {
                var result = new List<IntegrationEventContentDto>();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new IntegrationEventContentDto
                        {
                            EventId = reader.GetGuid(0),
                            EventTypeName = reader.GetString(1),
                            EventJsonString = reader.GetString(2)
                        });
                    }
                }

                return result;
            });
        }

        /// <inheritdoc />
        public Task<bool> AnyAsync(Expression<Func<IntegrationEventLog, bool>> condition)
        {
            Ensure.Any.IsNotNull(condition, nameof(condition));

            return DbContext.IntegrationEventLogs.AnyAsync(condition);
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> CreateAsync(IntegrationEvent @event)
        {
            Ensure.Any.IsNotNull(@event, nameof(@event));

            var eventLog = new IntegrationEventLog(@event);
            DbContext.IntegrationEventLogs.Add(eventLog);
            await DbContext.SaveChangesAsync();

            return eventLog;
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> MarkAsPublishedAsync(Guid eventId, string publisherId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));
            Ensure.String.IsNotNullOrEmpty(publisherId, nameof(publisherId));

            var eventLog = await DbContext
                .IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            eventLog.MarkAsPublished(publisherId);
            await UpdateAsync(eventLog);
            return eventLog;
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> MarkAsPublishAcknowledgmentTimeoutAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await DbContext
                .IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            eventLog.MarkAsPublishAcknowledgmentTimeout();
            await UpdateAsync(eventLog);
            return eventLog;
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> MarkAsCompletedAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await DbContext
                .IntegrationEventLogs
                .FirstOrDefaultAsync(log => log.EventId == eventId);
            if (eventLog == null)
            {
                throw new EventLogNotFoundException(eventId);
            }
            eventLog.MarkAsCompleted();
            await UpdateAsync(eventLog);
            return eventLog;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(List<Guid> eventIds)
        {
            Ensure.Collection.HasItems(eventIds, nameof(eventIds));

            var idParamNames = eventIds.Select((_, i) => $"@id{i}").ToList();
            var eventIdInClause = string.Join(", ", idParamNames);

            var sql = $@"
                DELETE FROM [IntegrationEventLogs] 
                WHERE [EventId] IN ({eventIdInClause})";

            var parameters = eventIds
                .Select((id, i) => new SqlParameter($"@id{i}", SqlDbType.UniqueIdentifier)
                {
                    Value = id
                })
                .Cast<DbParameter>()
                .ToList();

            await ExecuteWithConnectionAsync(sql, parameters, async command =>
            {
                await command.ExecuteNonQueryAsync();
                return true;
            });
        }

        private async Task UpdateAsync(IntegrationEventLog @event)
        {
            DbContext.IntegrationEventLogs.Update(@event);
            await DbContext.SaveChangesAsync();
        }

        private async Task<TResult> ExecuteWithConnectionAsync<TResult>(
            string sql,
            List<DbParameter> parameters,
            Func<DbCommand, Task<TResult>> executor)
        {
            var connection = DbContext.Database.GetDbConnection();
            var connectionInitiallyOpen = connection.State == ConnectionState.Open;

            try
            {
                if (!connectionInitiallyOpen)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddRange(parameters.ToArray());

                    return await executor(command);
                }
            }
            finally
            {
                if (!connectionInitiallyOpen && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
    }
}