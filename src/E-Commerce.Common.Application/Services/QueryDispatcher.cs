using E_Commerce.Common.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Common.Application.Services;

public class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public async Task<Result<TResponse>> DispatchAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return await handler.HandleAsync(query, cancellationToken);
    }
}