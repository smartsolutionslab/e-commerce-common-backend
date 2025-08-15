using MediatR;

namespace E_Commerce.Common.Domain.Primitives;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
