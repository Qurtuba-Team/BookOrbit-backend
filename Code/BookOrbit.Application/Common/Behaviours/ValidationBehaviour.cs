namespace BookOrbit.Application.Common.Behaviours;
public class ValidationBehaviour<TRequst, TResponse>(IValidator<TRequst>? validator = null)
    : IPipelineBehavior<TRequst, TResponse>
    where TRequst : IRequest<TResponse>
    where TResponse : IResult

{
    public async Task<TResponse> Handle(TRequst request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (validator is null)
            return await next(ct);

        var validationResult = await validator.ValidateAsync(request, ct);

        if (validationResult.IsValid)
            return await next(ct);

        var errors = validationResult.Errors
            .Select(error => Error.Validation(
        code: error.PropertyName,
        description: error.ErrorMessage))
            .ToList();

        return (dynamic)errors;
    }
}