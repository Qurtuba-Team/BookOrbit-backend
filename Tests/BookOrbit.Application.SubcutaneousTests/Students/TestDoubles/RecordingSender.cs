namespace BookOrbit.Application.SubcutaneousTests.Students.TestDoubles;

using System.Runtime.CompilerServices;
using MediatR;

internal sealed class RecordingSender : ISender
{
    public object? LastRequest { get; private set; }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return Task.FromResult(default(TResponse)!);
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return Task.FromResult<object?>(null);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        LastRequest = request;
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        yield break;
    }

    public async IAsyncEnumerable<object?> CreateStream(
        object request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        yield break;
    }
}
