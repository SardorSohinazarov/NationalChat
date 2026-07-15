using System.Linq.Expressions;
using Application.DataTransferObjects.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Extensions;

public static class QueryableExtensions
{
    public static async Task<CursorPagedResponse<TResult>> ToCursorPagedResponseAsync<TEntity, TResult>(
        this IQueryable<TEntity> query,
        CursorPaginationRequest pagination,
        Expression<Func<TEntity, int>> idSelector,
        Expression<Func<TEntity, TResult>> resultSelector,
        Func<TResult, int> cursorSelector,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        if (pagination.BeforeId is not null)
        {
            var beforeId = Expression.Lambda<Func<TEntity, bool>>(
                Expression.LessThan(idSelector.Body, Expression.Constant(pagination.BeforeId.Value)),
                idSelector.Parameters);
            query = query.Where(beforeId);
        }

        var results = await query.OrderByDescending(idSelector)
            .Take(pagination.Limit + 1)
            .Select(resultSelector)
            .ToListAsync(cancellationToken);

        var hasMore = results.Count > pagination.Limit;
        IReadOnlyList<TResult> items = hasMore ? results.Take(pagination.Limit).ToArray() : results;
        int? nextCursor = hasMore ? cursorSelector(items[^1]) : null;

        return new(items, nextCursor, hasMore);
    }
}
