# NetKit.EventDrivenCommunication

NetKit.EventDrivenCommunication is a messaging component that enables Pub\Sub communication between system components or services.

# Structure

1. ```Softeq.NetKit.Components.EventBus``` - exposes core Pub\Sub abstractions and models of the component. 
2. ```Softeq.NetKit.Components.EventBus.Service``` - Pub\Sub implementation that uses Azure Service Bus as a communication mechanins.
3. ```Softeq.NetKit.Integrations.EventLog``` - SQL DB Store for event state. 

# Geting Started

## Install 
1. Check-out master branch from repository
2. Add a reference to Softeq.NetKit.Components.EventBus into target project.
3. Add a reference to Softeq.NetKit.Components.EventBus.Service into target project.
4. (Optional) Add a reference to Softeq.NetKit.Integrations.EventLog into target project.

## Develop

1. Create custom event class inherited from ```IntegrationEvent``` base class
```csharp
    public class UserCreatedEvent : IntegrationEvent
	{
		public UserCreatedEvent(Guid userId)
		{
		    UserId = userId;
		}

		public Guid UserId { get; set; }
	}
```

2. Implement ```IEventHandler<>``` interface to handle received custom events
```csharp
    public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
	    public async Task Handle(UserCreatedEvent @event)
	    {
		    //Code goes here
	    }
    }

```

## Configure DI Container

1. Set up and register ServiceBus configuration ```ServiceBusPersisterConnectionConfiguration``` in your DI container
```csharp
    container.Register(x =>
    {
        var context = x.Resolve<IComponentContext>();
        var config = context.Resolve<IConfiguration>();
        return new ServiceBusPersisterConnectionConfiguration
        {
            ConnectionString = config["CONN_STR"],
            TopicConfiguration = new ServiceBusPersisterTopicConnectionConfiguration
            {
                TopicName = config["TOPIC"],
                SubscriptionName = config["SUBSCRIPTION"] 
            },
            QueueConfiguration = new ServiceBusPersisterQueueConnectionConfiguration
            {
                QueueName = config["QUEUE"]
            }
        };
    }).As<ServiceBusPersisterConnectionConfiguration>();

```

2. Register ```IServiceBusPersisterConnection``` implementation
```csharp
    builder.RegisterType<ServiceBusPersisterConnection>()
        .As<IServiceBusPersisterConnection>();

```

3. Set up and register Service Bus message configuration ```MessageQueueConfiguration```
```csharp
    builder.Register(context =>
    {
        var config = context.Resolve<IConfiguration>();

        return new MessageQueueConfiguration
        {
            TimeToLive = Convert.ToInt32(config["MSG_TTL"])
        };
    })
```

4. Register ```IEventBusSubscriptionsManager``` implementation
```csharp
    builder.RegisterType<EventBusSubscriptionsManager>()
                .As<IEventBusSubscriptionsManager>();

```

5. Register ```IEventBusService``` implementation
```csharp
    builder.RegisterType<EventBusService>()
                .As<IEventBusPublisher>();
    builder.RegisterType<EventBusService>()
                .As<IEventBusSubscriber>();

``` 

6. (Optional) Register ```IIntegrationEventLogService``` implementation
```csharp
    builder.RegisterType<IntegrationEventLogService>()
                .As<IIntegrationEventLogService>();

```

## Configure service

1. Enable the listeners on desired event sources
```csharp
    IEventBusSubscriber eventBus;
    eventBus.RegisterQueueListener();
    eventBus.RegisterSubscriptionListenerAsync().GetAwaiter().GetResult();
```

2. Register your custom event handlers
```csharp
    eventBus.SubscribeAsync<UserCreatedEvent, UserCreatedEventHandler>().GetAwaiter().GetResult();
```

## Use

Inject ```IEventBusPublisher``` and ```IIntegrationEventLogService``` into your service

```csharp
    public class SomeService
    {
        private readonly IEventBusPublisher _eventPublisher;
        private readonly IIntegrationEventLogService _eventLogService;

        public SomeService(IEventBusPublisher eventPublisher, IIntegrationEventLogService eventLogService)
        {
            _eventPublisher = eventPublisher;
            _eventLogService = eventLogService;
        }

        public async Task Do(UserCreatedEvent @event)
        {
            await _eventLogService.SaveAsync(@event);
            await _eventPublisher.PublishToTopicAsync(@event, delayInSeconds);
            await _eventLogService.MarkAsPublishedAsync(@event);
        }
    }
```

## About

This project is maintained by Softeq Development Corp.

We specialize in .NET core applications.

## Contributing

We welcome any contributions.

## License

The Query Utils project is available for free use, as described by the [LICENSE](/LICENSE) (MIT).