using E_Commerce.Common.Application.Abstractions;

namespace E_Commerce.Common.Application.Services;

public interface IQueryDispatcher
{
    Task<Result<TResponse>> DispatchAsync<TQuery, TResponse>(TQuery query,
        CancellationToken cancellationToken = default) where TQuery : IQuery<TResponse>;
}