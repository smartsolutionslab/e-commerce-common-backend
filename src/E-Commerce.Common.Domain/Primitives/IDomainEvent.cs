using MediatR;

namespace E_Commerce.Common.Domain.Primitives;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
