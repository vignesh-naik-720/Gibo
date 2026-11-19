using System.Collections.Concurrent;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gibo.Application.Common.Behaviors;

public interface IIdempotentRequest
{
    string? IdempotencyKey { get; }
}

/// <summary>
/// Pipeline Behavior guaranteeing idempotency on state-modifying requests.
/// Prevents mobile cellular retry duplicates from generating duplicate users or consent records.
/// </summary>
public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ConcurrentDictionary<string, object> _cache = new();
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

    public IdempotencyBehavior(ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentRequest idempotentReq || string.IsNullOrWhiteSpace(idempotentReq.IdempotencyKey))
        {
            return await next();
        }

        string key = idempotentReq.IdempotencyKey;

        if (_cache.TryGetValue(key, out var cachedResponse) && cachedResponse is TResponse typedResponse)
        {
            _logger.LogInformation("[IDEMPOTENCY_HIT] Returning cached response for key: {Key}", key);
            return typedResponse;
        }

        var response = await next();
        if (response != null)
        {
            _cache.TryAdd(key, response);
        }

        return response;
    }
}
