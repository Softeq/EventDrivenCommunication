﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Softeq.NetKit.Integrations.EventLog;

namespace Softeq.NetKit.Integrations.EventLog.Migrations
{
    [DbContext(typeof(IntegrationEventLogContext))]
    partial class IntegrationEventLogContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("dbo")
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Softeq.NetKit.Integrations.EventLog.IntegrationEventLog", b =>
                {
                    b.Property<Guid>("EventEnvelope_Id");

                    b.Property<int>("EventState");

                    b.Property<int>("TimesSent");

                    b.Property<DateTimeOffset?>("Updated");

                    b.HasKey("EventEnvelope_Id");

                    b.HasIndex("EventState");

                    b.ToTable("IntegrationEventLogs");
                });

            modelBuilder.Entity("Softeq.NetKit.Integrations.EventLog.IntegrationEventLog", b =>
                {
                    b.OwnsOne("Softeq.NetKit.Components.EventBus.Events.IntegrationEventEnvelope", "EventEnvelope", b1 =>
                        {
                            b1.Property<Guid>("IntegrationEventLogEventEnvelope_Id");

                            b1.Property<string>("CorrelationId");

                            b1.Property<DateTimeOffset>("Created");

                            b1.Property<string>("Event")
                                .IsRequired();

                            b1.Property<Guid>("Id")
                                .HasColumnName("EventEnvelope_Id");

                            b1.Property<string>("PublisherId")
                                .IsRequired();

                            b1.Property<string>("SequenceId");

                            b1.HasKey("IntegrationEventLogEventEnvelope_Id");

                            b1.HasIndex("Created");

                            b1.HasIndex("PublisherId");

                            b1.HasIndex("SequenceId");

                            b1.ToTable("IntegrationEventLogs","dbo");

                            b1.HasOne("Softeq.NetKit.Integrations.EventLog.IntegrationEventLog")
                                .WithOne("EventEnvelope")
                                .HasForeignKey("Softeq.NetKit.Components.EventBus.Events.IntegrationEventEnvelope", "IntegrationEventLogEventEnvelope_Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
