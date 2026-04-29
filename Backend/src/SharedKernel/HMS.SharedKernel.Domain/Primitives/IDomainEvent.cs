using MediatR;

namespace HMS.SharedKernel.Primitives;

/// <summary>
/// Marker for domain events. Implements MediatR's INotification so that
/// domain events can be dispatched via IPublisher without any infrastructure
/// dependency in the Domain layer.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid Id         { get; }
    DateTime OccurredOn { get; }
}
