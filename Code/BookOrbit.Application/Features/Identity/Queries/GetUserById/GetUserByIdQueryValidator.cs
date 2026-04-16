namespace BookOrbit.Application.Features.Identity.Queries.GetUserById;
public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(IdentityApplicationErrors.IdRequired.Description);
    }
}