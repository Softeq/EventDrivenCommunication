using Microsoft.EntityFrameworkCore.Migrations;

namespace Softeq.NetKit.Integrations.EventLog.Migrations
{
    public partial class AddSessionIdToIntegrationEventLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                schema: "dbo",
                table: "IntegrationEventLogs");
        }
    }
}
