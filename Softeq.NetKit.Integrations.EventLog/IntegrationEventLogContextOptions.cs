// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.NetKit.Integrations.EventLog
{
    public class IntegrationEventLogContextOptions
    {
        public string Schema { get; set; } = "dbo";

        public string TableName { get; set; } = "IntegrationEventLogs";
    }
}
