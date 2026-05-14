namespace BookOrbit.Application.Common.Interfaces.GetAllQueryHandlers;
public interface IPagedQuery<TDto> : ICachedQuery<Result<PaginatedList<TDto>>> where TDto : class
{
    int Page { get; }
    int PageSize { get; }

    string? SortColumn { get; } 
    string? SortDirection { get; } 

    string? SearchTerm { get; }
}