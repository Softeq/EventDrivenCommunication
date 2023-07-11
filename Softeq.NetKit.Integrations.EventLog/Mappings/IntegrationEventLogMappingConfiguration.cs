﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Softeq.NetKit.Components.EventBus.Events;
using Softeq.NetKit.Integrations.EventLog.Abstract;
using Softeq.NetKit.Integrations.EventLog.Utility;

namespace Softeq.NetKit.Integrations.EventLog.Mappings
{
    internal class IntegrationEventLogMappingConfiguration : DomainModelBuilder<IntegrationEventLog>, IEntityMappingConfiguration
    {
        public override void Build(EntityTypeBuilder<IntegrationEventLog> builder)
        {
            builder.Property(eventLog => eventLog.EventTypeName).IsRequired();
            builder.Property(eventLog => eventLog.EventState).HasConversion<int>().IsRequired();
            builder.HasIndex(eventLog => eventLog.EventState).IsUnique(false);
            builder.Property(eventLog => eventLog.TimesSent).IsRequired();
            builder.OwnsOne(entity => entity.Content, ownershipBuilder =>
                {
                    ownershipBuilder.Property(x => x.Created).IsRequired();
                    ownershipBuilder.HasIndex(x => x.Created).IsUnique(false);
                    ownershipBuilder.Property(x => x.PublisherId).IsRequired();
                    ownershipBuilder.HasIndex(x => x.PublisherId).IsUnique(false);
                    ownershipBuilder.Property(x => x.SessionId).IsRequired(false);
                    ownershipBuilder.HasIndex(x => x.SessionId).IsUnique(false);
                    ownershipBuilder.Property(x => x.CorrelationId).IsRequired(false);

                    var serializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        ContractResolver = new PrivateFieldContractResolver()
                    };
                    ownershipBuilder.Property(x => x.Event)
                        .HasConversion(
                            @event => JsonConvert.SerializeObject(@event, serializerSettings),
                            json => (IntegrationEvent)JsonConvert.DeserializeObject(json, serializerSettings))
                        .IsRequired();

                    // Use Content.Id as Primary Key for IntegrationEventLog table
                    var identityProperty = ownershipBuilder.OwnedEntityType.ClrType.GetProperties()
                        .Single(p => p.Name == nameof(IntegrationEventLog.Content.Id));
                    var propertyType = identityProperty.PropertyType;
                    var identityPropertyName = $"{nameof(IntegrationEventLog.Content)}_{nameof(IntegrationEventLog.Content.Id)}";
                    ownershipBuilder.Property(propertyType, identityProperty.Name).HasColumnName(identityPropertyName);
                    builder.Property(propertyType, identityPropertyName).ValueGeneratedNever();
                    builder.HasKey(identityPropertyName);
                });
        }
    }
}