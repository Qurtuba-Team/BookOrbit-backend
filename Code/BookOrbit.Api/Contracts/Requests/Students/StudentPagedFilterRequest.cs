
namespace BookOrbit.Api.Contracts.Requests.Students;
public record StudentPagedFilterRequest : PagedFilterRequest
{
    public List<StudentState>? States { get; set; } = null;
    public bool? EmailConfirmed { get; set; } = null;
}