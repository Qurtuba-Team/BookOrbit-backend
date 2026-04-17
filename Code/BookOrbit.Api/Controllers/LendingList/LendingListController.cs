namespace BookOrbit.Api.Controllers.LendingList;

[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[Authorize]

public class LendingListController(
    ISender sender) : ApiController
{
    
}