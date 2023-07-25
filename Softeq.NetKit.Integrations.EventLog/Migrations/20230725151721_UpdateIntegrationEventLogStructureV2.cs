using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Softeq.NetKit.Integrations.EventLog.Migrations
{
    public partial class UpdateIntegrationEventLogStructureV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IntegrationEventLogs",
                schema: "dbo",
                table: "IntegrationEventLogs");


            //migrationBuilder.DropColumn(
            //    name: "EventId",
            //    schema: "dbo",
            //    table: "IntegrationEventLogs");
            //migrationBuilder.AddColumn<Guid>(
            //    name: "EventEnvelope_Id",
            //    schema: "dbo",
            //    table: "IntegrationEventLogs",
            //    nullable: false,
            //    defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            migrationBuilder.RenameColumn(
                name: "EventId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "EventEnvelope_Id");
            ////////migrationBuilder.AddPrimaryKey(
            ////////    name: "PK_IntegrationEventLogs",
            ////////    schema: "dbo",
            ////////    table: "IntegrationEventLogs",
            ////////    column: "EventEnvelope_Id");


            //migrationBuilder.DropColumn(
            //    name: "Content",
            //    schema: "dbo",
            //    table: "IntegrationEventLogs");
            //migrationBuilder.AddColumn<string>(
            //    name: "EventEnvelope_Event",
            //    schema: "dbo",
            //    table: "IntegrationEventLogs",
            //    nullable: false,
            //    defaultValue: "");
            migrationBuilder.RenameColumn(
                name: "Content",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "EventEnvelope_Event");


            migrationBuilder.RenameColumn(
                name: "Created",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "EventEnvelope_Created");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "EventEnvelope_SequenceId");

            //migrationBuilder.RenameColumn(
            //    name: "EventTypeName",
            //    schema: "dbo",
            //    table: "IntegrationEventLogs",
            //    newName: "EventEnvelope_PublisherId");
            migrationBuilder.DropColumn(
                name: "EventTypeName",
                schema: "dbo",
                table: "IntegrationEventLogs");
            migrationBuilder.AddColumn<string>(
                name: "EventEnvelope_PublisherId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: false,
                defaultValue: "");
            migrationBuilder.Sql(@"
DECLARE @PublisherId UNIQUEIDENTIFIER;
SET @PublisherId = (
    SELECT TOP 1 JsonData.PublisherId
    FROM IntegrationEventLogs
    CROSS APPLY OPENJSON(IntegrationEventLogs.EventEnvelope_Event, N'$') WITH (PublisherId UNIQUEIDENTIFIER N'$.PublisherId') AS JsonData);
UPDATE IntegrationEventLogs SET EventEnvelope_PublisherId = @PublisherId;");

            migrationBuilder.RenameIndex(
                name: "IX_IntegrationEventLogs_SessionId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "IX_IntegrationEventLogs_EventEnvelope_SequenceId");

            migrationBuilder.RenameIndex(
                name: "IX_IntegrationEventLogs_Created",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "IX_IntegrationEventLogs_EventEnvelope_Created");

            migrationBuilder.AlterColumn<string>(
                name: "EventEnvelope_PublisherId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: false,
                oldClrType: typeof(string));

           
            migrationBuilder.AddColumn<string>(
                name: "EventEnvelope_CorrelationId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: true);




            migrationBuilder.AddColumn<Guid>(
                name: "EventLogId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            migrationBuilder.Sql("UPDATE IntegrationEventLogs SET EventLogId = NEWID();");
            migrationBuilder.AddPrimaryKey(
                name: "PK_IntegrationEventLogs",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "EventLogId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventLogs_EventEnvelope_Id",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "EventEnvelope_Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventLogs_EventEnvelope_PublisherId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "EventEnvelope_PublisherId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IntegrationEventLogs",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationEventLogs_EventEnvelope_Id",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationEventLogs_EventEnvelope_PublisherId",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropColumn(
                name: "EventLogId",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropColumn(
                name: "EventEnvelope_CorrelationId",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropColumn(
                name: "EventEnvelope_Event",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.DropColumn(
                name: "EventEnvelope_Id",
                schema: "dbo",
                table: "IntegrationEventLogs");

            migrationBuilder.RenameColumn(
                name: "EventEnvelope_Created",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "EventEnvelope_SequenceId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "EventEnvelope_PublisherId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "EventTypeName");

            migrationBuilder.RenameIndex(
                name: "IX_IntegrationEventLogs_EventEnvelope_SequenceId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "IX_IntegrationEventLogs_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_IntegrationEventLogs_EventEnvelope_Created",
                schema: "dbo",
                table: "IntegrationEventLogs",
                newName: "IX_IntegrationEventLogs_Created");

            migrationBuilder.AlterColumn<string>(
                name: "EventTypeName",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Content",
                schema: "dbo",
                table: "IntegrationEventLogs",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IntegrationEventLogs",
                schema: "dbo",
                table: "IntegrationEventLogs",
                column: "EventId");
        }
    }
}
