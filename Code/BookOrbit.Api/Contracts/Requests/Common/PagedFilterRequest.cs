namespace BookOrbit.Api.Contracts.Requests.Common
{
    public record PagedFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 1;
        public string? SearchTerm { get; set; } = null;
        public string? SortColumn { get; set; } = null;
        public string? SortDirection { get; set; } = null;
    }
}
