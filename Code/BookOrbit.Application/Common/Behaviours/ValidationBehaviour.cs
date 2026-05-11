namespace BookOrbit.Application.Common.Behaviours;
public class ValidationBehaviour<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult, IErrorFactory<TResponse>

{
    public async Task<TResponse> Handle(TRequest request,
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

        return TResponse.FromErrors(errors);
    }
}