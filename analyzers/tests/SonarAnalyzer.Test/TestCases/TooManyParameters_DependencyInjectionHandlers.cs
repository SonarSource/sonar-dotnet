using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;

public class Message { }
public class Command : IRequest { }
public class Query : IRequest<int> { }
public class Event : INotification { }

public class MessageConsumer : IConsumer<Message>
{
    public MessageConsumer(int p1, int p2, int p3, int p4) { } // Compliant

    public Task Consume(ConsumeContext<Message> context) => Task.CompletedTask;

    public void Method(int p1, int p2, int p3, int p4) { } // Noncompliant
}

public class CommandHandler : IRequestHandler<Command>     // IRequestHandler<TRequest> does not inherit IRequestHandler<TRequest, TResponse>
{
    public CommandHandler(int p1, int p2, int p3, int p4) { } // Compliant

    public Task Handle(Command request, CancellationToken cancellationToken) => Task.CompletedTask;
}

public class QueryHandler : IRequestHandler<Query, int>
{
    public QueryHandler(int p1, int p2, int p3, int p4) { } // Compliant

    public Task<int> Handle(Query request, CancellationToken cancellationToken) => Task.FromResult(0);
}

public class EventNotificationHandler : INotificationHandler<Event>
{
    public EventNotificationHandler(int p1, int p2, int p3, int p4) { } // Compliant

    public Task Handle(Event notification, CancellationToken cancellationToken) => Task.CompletedTask;
}

public class QueryBehavior : IPipelineBehavior<Query, int>
{
    public QueryBehavior(int p1, int p2, int p3, int p4) { } // Compliant

    public Task<int> Handle(Query request, RequestHandlerDelegate<int> next, CancellationToken cancellationToken) => next();
}

public class NotAHandler
{
    public NotAHandler(int p1, int p2, int p3, int p4) { } // Noncompliant
}
