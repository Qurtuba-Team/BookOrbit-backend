using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BookOrbit.Application.UnitTests.Helpers;

/// <summary>
/// Creates a queryable fake <see cref="DbSet{T}"/> backed by an in-memory list.
/// Supports LINQ-to-Objects (sufficient for unit testing Application handlers
/// that call AsNoTracking / ToListAsync / AnyAsync / Select).
/// </summary>
public static class MockDbSetHelper
{
    public static DbSet<T> CreateDbSet<T>(IList<T> data) where T : class
    {
        var queryable = data.AsQueryable();

        var fakeDbSet = A.Fake<DbSet<T>>(options =>
            options.Implements<IQueryable<T>>()
                   .Implements<IAsyncEnumerable<T>>());

        // IQueryable wiring
        A.CallTo(() => ((IQueryable<T>)fakeDbSet).Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        A.CallTo(() => ((IQueryable<T>)fakeDbSet).Expression)
            .Returns(queryable.Expression);
        A.CallTo(() => ((IQueryable<T>)fakeDbSet).ElementType)
            .Returns(queryable.ElementType);
        A.CallTo(() => ((IQueryable<T>)fakeDbSet).GetEnumerator())
            .Returns(queryable.GetEnumerator());

        // IAsyncEnumerable wiring
        A.CallTo(() => ((IAsyncEnumerable<T>)fakeDbSet).GetAsyncEnumerator(A<CancellationToken>._))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        return fakeDbSet;
    }

    // ── Async provider infrastructure ────────────────────────────────────────

    private class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression) => inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments().First();
            var executeMethod = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute))!
                .MakeGenericMethod(resultType);

            var result = executeMethod.Invoke(inner, [expression]);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                                        .MakeGenericMethod(resultType)
                                        .Invoke(null, [result])!;
        }
    }

    private class TestAsyncEnumerable<T>(Expression expression)
        : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    private class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask<bool> MoveNextAsync()
            => ValueTask.FromResult(inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
