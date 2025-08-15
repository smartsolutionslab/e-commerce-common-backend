using MediatR;

namespace E_Commerce.Common.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
