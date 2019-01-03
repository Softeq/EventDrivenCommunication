// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Softeq.NetKit.Integrations.EventLog.Utility
{
    public class EventLogTransaction : IDisposable
    {
        private readonly DbContext _context;

        private EventLogTransaction(DbContext context) =>
            _context = context ?? throw new ArgumentNullException(nameof(context));

        public static EventLogTransaction New(DbContext context) =>
            new EventLogTransaction(context);

        public async Task ExecuteAsync(Func<Task> action)
        {
            // Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    await action();
                    transaction.Commit();
                }
            });
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
