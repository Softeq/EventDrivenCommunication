// Developed by Softeq Development Corporation
// http://www.softeq.com

using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Dtos;
using Softeq.NetKit.Integrations.EventLog.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SortOrder = Softeq.NetKit.Integrations.EventLog.Dtos.SortOrder;

namespace Softeq.NetKit.Integrations.EventLog
{
    // Backward-compatible non-generic service for existing consumers/DI registrations.
    public class IntegrationEventLogService : IntegrationEventLogService<IntegrationEventLogContext>
    {
        public IntegrationEventLogService(Func<IntegrationEventLogContext> dbContextFactory)
            : base(dbContextFactory, Options.Create(new IntegrationEventLogContextOptions()))
        {
        }
    }

    public class IntegrationEventLogService<TContext> : IIntegrationEventLogService
        where TContext : DbContext
    {
        private readonly Func<TContext> _dbContextFactory;

        protected TContext DbContext => _dbContextFactory.Invoke();
        private readonly IOptions<IntegrationEventLogContextOptions> _integrationEventLogContextOptions;

        public IntegrationEventLogService(
            Func<TContext> dbContextFactory,
            IOptions<IntegrationEventLogContextOptions> options)
        {
            _dbContextFactory = Ensure.Any.IsNotNull(dbContextFactory, nameof(dbContextFactory));
            _integrationEventLogContextOptions = Ensure.Any.IsNotNull(options, nameof(options));
        }

        private DbSet<IntegrationEventLog> IntegrationEventLogs => DbContext.Set<IntegrationEventLog>();

        private string FullTableName => $"{_integrationEventLogContextOptions.Value.Schema}.{_integrationEventLogContextOptions.Value.TableName}";

        /// <inheritdoc />
        public async Task<IntegrationEventLog> GetAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await IntegrationEventLogs
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

            return IntegrationEventLogs.Where(condition).ToListAsync();
        }

        /// <inheritdoc />
        public Task<List<IntegrationEventContentDto>> GetEventContentListAsync(
            List<EventState> eventStates,
            DateTimeOffset createdUntil,
            SortOrder createdDateOrder = SortOrder.Ascending,
            int skipCount = 0,
            int takeCount = 100,
            CancellationToken cancellationToken = default)
        {
            Ensure.Any.IsNotNull(eventStates, nameof(eventStates));
            Ensure.Collection.HasItems(eventStates, nameof(eventStates));
            Ensure.Comparable.IsGte(skipCount, 0, nameof(skipCount));
            Ensure.Comparable.IsGt(takeCount, 0, nameof(takeCount));

            var stateParamNames = eventStates.Select((_, i) => $"@state{i}").ToList();
            var eventStateInClause = string.Join(", ", stateParamNames);
            var orderClause = createdDateOrder == SortOrder.Ascending ? "ASC" : "DESC";

            // Raw SQL is used instead of EF Core LINQ to avoid deserialization issues for old events
            var sql = $@"
                SELECT [EventId], [EventTypeName], [Content], [Created]
                FROM {FullTableName}
                WHERE [EventState] IN ({eventStateInClause}) AND [Created] <= @createdUntil
                ORDER BY [Created] {orderClause}
                OFFSET @skipCount ROWS
                FETCH NEXT @takeCount ROWS ONLY;";

            var parameters = new List<DbParameter>
            {
                new SqlParameter("@skipCount", SqlDbType.Int) { Value = skipCount },
                new SqlParameter("@takeCount", SqlDbType.Int) { Value = takeCount },
                new SqlParameter("@createdUntil", SqlDbType.DateTimeOffset) { Value = createdUntil }
            };
            parameters.AddRange(
                eventStates
                    .Select((state, i) => new SqlParameter($"@state{i}", SqlDbType.Int)
                    {
                        Value = (int)state
                    }));

            return ExecuteWithConnectionAsync(sql, parameters, async command =>
            {
                var result = new List<IntegrationEventContentDto>();

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        result.Add(new IntegrationEventContentDto
                        {
                            EventId = reader.GetGuid(0),
                            EventTypeName = reader.GetString(1),
                            EventJsonString = reader.GetString(2),
                            Created = reader.GetFieldValue<DateTimeOffset>(3)
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

            return IntegrationEventLogs.AnyAsync(condition);
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> CreateAsync(IntegrationEvent @event)
        {
            Ensure.Any.IsNotNull(@event, nameof(@event));

            var eventLog = new IntegrationEventLog(@event);
            IntegrationEventLogs.Add(eventLog);
            await DbContext.SaveChangesAsync();

            return eventLog;
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> MarkAsPublishedAsync(Guid eventId, string publisherId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));
            Ensure.String.IsNotNullOrEmpty(publisherId, nameof(publisherId));

            var eventLog = await IntegrationEventLogs
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
        public async Task<IList<IntegrationEventLog>> MarkAsPublishedAsync(IList<Guid> eventIds, string publisherId)
        {
            Ensure.Collection.HasItems(eventIds, nameof(eventIds));
            Ensure.String.IsNotNullOrEmpty(publisherId, nameof(publisherId));

            var eventLogs = await IntegrationEventLogs
                .Where(log => eventIds.Contains(log.EventId))
                .ToListAsync();
            foreach (var eventLog in eventLogs)
            {
                eventLog.MarkAsPublished(publisherId);
            }
            await UpdateAsync(eventLogs);
            return eventLogs;
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> MarkAsPublishAcknowledgmentTimeoutAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await IntegrationEventLogs
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
        public async Task<IList<IntegrationEventLog>> MarkAsPublishAcknowledgmentTimeoutAsync(IList<Guid> eventIds)
        {
            Ensure.Collection.HasItems(eventIds, nameof(eventIds));

            var eventLogs = await IntegrationEventLogs
                .Where(log => eventIds.Contains(log.EventId))
                .ToListAsync();
            foreach (var eventLog in eventLogs)
            {
                eventLog.MarkAsPublishAcknowledgmentTimeout();
            }
            await UpdateAsync(eventLogs);
            return eventLogs;
        }

        /// <inheritdoc />
        public async Task<IntegrationEventLog> MarkAsCompletedAsync(Guid eventId)
        {
            Ensure.Guid.IsNotEmpty(eventId, nameof(eventId));

            var eventLog = await IntegrationEventLogs
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
        public async Task<IList<IntegrationEventLog>> MarkAsCompletedAsync(IList<Guid> eventIds)
        {
            Ensure.Collection.HasItems(eventIds, nameof(eventIds));

            var eventLogs = await IntegrationEventLogs
                .Where(log => eventIds.Contains(log.EventId))
                .ToListAsync();
            foreach (var eventLog in eventLogs)
            {
                eventLog.MarkAsCompleted();
            }
            await UpdateAsync(eventLogs);
            return eventLogs;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(IReadOnlyCollection<Guid> eventIds)
        {
            Ensure.Collection.HasItems(eventIds, nameof(eventIds));

            var idParamNames = eventIds.Select((_, i) => $"@id{i}").ToList();
            var eventIdInClause = string.Join(", ", idParamNames);

            // Raw SQL is used instead of EF Core LINQ to avoid deserialization issues for old events
            var sql = $@"
                DELETE FROM {FullTableName} 
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

        private async Task UpdateAsync(IntegrationEventLog eventLog)
        {
            IntegrationEventLogs.Update(eventLog);
            await DbContext.SaveChangesAsync();
        }

        private async Task UpdateAsync(IList<IntegrationEventLog> eventLogs)
        {
            IntegrationEventLogs.UpdateRange(eventLogs);
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