using Microsoft.EntityFrameworkCore.Migrations;

namespace Softeq.NetKit.Integrations.EventLog.Migrations
{
    public partial class UpdateIntegrationEventLogStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationEventLogs_EventState_StateId",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropTable(
                name: "EventState",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationEventLogs_StateId",
                schema: "dbo",
                table: "IntegrationEventLogs");
            
            migrationBuilder.RenameColumn(
                name: "StateId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "EventState");

            migrationBuilder.RenameColumn(
                name: "UpdatedTime",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "Updated");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "Created");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventLogs_Created",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventLogs_EventState",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "EventState");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventLogs_SessionId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "SessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IntegrationEventLogs_Created",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationEventLogs_EventState",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationEventLogs_SessionId",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.RenameColumn(
                name: "EventState",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "StateId");

            migrationBuilder.DropColumn(
                name: "SessionId",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.RenameColumn(
                name: "Updated",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "UpdatedTime");

            migrationBuilder.RenameColumn(
                name: "Created",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "CreationTime");

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

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationEventLogs_EventState_StateId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "StateId",
                principalSchema: "dbo",
                principalTable: "EventState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
