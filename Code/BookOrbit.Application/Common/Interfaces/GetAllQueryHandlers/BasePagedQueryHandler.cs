namespace BookOrbit.Application.Common.Interfaces.GetAllQueryHandlers;
    public abstract class BasePagedQueryHandler<TEntity, TDto, TQuery>
        : IRequestHandler<TQuery, Result<PaginatedList<TDto>>>
    where TEntity : Entity
    where TDto : class
    where TQuery : IPagedQuery<TDto>
{
    protected abstract Dictionary<
        string,
    Func<IQueryable<TEntity>,
     bool,
     IOrderedQueryable<TEntity>>
        > SortMappings
    {
        get;
    }

    public async Task<Result<PaginatedList<TDto>>> Handle(
        TQuery query,
        CancellationToken ct)
    {
        IQueryable<TEntity> entityQuery = GetBaseQuery();

        entityQuery = ApplyFilters(entityQuery, query);

        entityQuery = ApplySearch(entityQuery, query);

        entityQuery = ApplySorting(entityQuery, query);

        int count = await entityQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        entityQuery = entityQuery.ApplyPagination(page, pageSize);

        var items = await ProjectToDto(entityQuery)
            .ToListAsync(ct);

        return new PaginatedList<TDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    protected abstract IQueryable<TEntity> GetBaseQuery();

    protected virtual IQueryable<TEntity> ApplyFilters(
        IQueryable<TEntity> query,
        TQuery request)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> ApplySearch(
        IQueryable<TEntity> query,
        TQuery request)
    {
        return query;
    }

    private IQueryable<TEntity> ApplySorting(
      IQueryable<TEntity> query,
      TQuery request)
    {
        string sortColumn =
            string.IsNullOrWhiteSpace(request.SortColumn)
                ? "id"
                : request.SortColumn.ToLower();

        bool isDescending =
            string.IsNullOrWhiteSpace(request.SortDirection)
            || request.SortDirection.Equals(
                "desc",
                StringComparison.OrdinalIgnoreCase);

        if (!SortMappings.TryGetValue(sortColumn, out var sortFunc))
        {
            return query.OrderByDescending(e => e.Id);
        }

        return sortFunc(query, isDescending);
    }

    protected abstract IQueryable<TDto> ProjectToDto(
        IQueryable<TEntity> query);
}
