

namespace BookOrbit.Api.Controllers;


[ApiController]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
public class ApiController : ControllerBase
{
    protected ActionResult Problem(List<Error> errors,HttpContext context)
    {
        var problem = ErrorToProblemMapper.Map(errors,context);
        return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
    }
}

