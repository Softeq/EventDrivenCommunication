using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Softeq.NetKit.Integrations.EventLog.Migrations
{
    public partial class AddInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "EventState",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationEventLogs",
                schema: "dbo",
                columns: table => new
                {
                    EventId = table.Column<Guid>(nullable: false),
                    EventTypeName = table.Column<string>(nullable: false),
                    StateId = table.Column<int>(nullable: false),
                    TimesSent = table.Column<int>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEventLogs", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_IntegrationEventLogs_EventState_StateId",
                        column: x => x.StateId,
                        principalSchema: "dbo",
                        principalTable: "EventState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "EventState",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Published" },
                    { 3, "Completed" },
                    { 2, "PublishedFailed" },
                    { 0, "NotPublished" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventLogs_StateId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "StateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationEventLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EventState",
                schema: "dbo");
        }
    }
}
