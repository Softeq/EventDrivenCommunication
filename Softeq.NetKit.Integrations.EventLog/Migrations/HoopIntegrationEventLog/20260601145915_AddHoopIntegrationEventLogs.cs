using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Softeq.NetKit.Integrations.EventLog.Migrations.HoopIntegrationEventLog
{
    public partial class AddHoopIntegrationEventLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "HoopIntegrationEventLogs",
                schema: "dbo",
                columns: table => new
                {
                    EventId = table.Column<Guid>(nullable: false),
                    EventTypeName = table.Column<string>(nullable: false),
                    EventState = table.Column<int>(nullable: false),
                    TimesSent = table.Column<int>(nullable: false),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    Updated = table.Column<DateTimeOffset>(nullable: true),
                    SessionId = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoopIntegrationEventLogs", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoopIntegrationEventLogs_Created",
                schema: "dbo",
                table: "HoopIntegrationEventLogs",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_HoopIntegrationEventLogs_EventState",
                schema: "dbo",
                table: "HoopIntegrationEventLogs",
                column: "EventState");

            migrationBuilder.CreateIndex(
                name: "IX_HoopIntegrationEventLogs_SessionId",
                schema: "dbo",
                table: "HoopIntegrationEventLogs",
                column: "SessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoopIntegrationEventLogs",
                schema: "dbo");
        }
    }
}
