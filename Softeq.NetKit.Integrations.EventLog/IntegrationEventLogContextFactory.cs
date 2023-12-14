// Developed by Softeq Development Corporation
// http://www.softeq.com

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Softeq.NetKit.Integrations.EventLog
{
    /// <summary>
    /// This factory is needed to create db migrations in class library via Package Manager Console
    /// https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#from-a-design-time-factory
    /// To use this factory
    /// 1. Build solution
    /// 2. Set the current project as startup project
    /// 3. Edit the project file: change the project TargetFramework to, for example, <TargetFramework>netcoreapp3.1</TargetFramework>
    /// 4. Open Package Manager Console. Set current project as the default project
    /// 5. Create migration by running command: add-migration migration-name -context IntegrationEventLogContext
    /// </summary>
    public class IntegrationEventLogContextFactory : IDesignTimeDbContextFactory<IntegrationEventLogContext>
    {
        public IntegrationEventLogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IntegrationEventLogContext>();
            optionsBuilder.UseSqlServer("data source=.\\SQLEXPRESS");
            return new IntegrationEventLogContext(optionsBuilder.Options);
        }
    }
}
