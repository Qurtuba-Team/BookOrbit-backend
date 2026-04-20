namespace BookOrbit.Application.Common.Interfaces;
public interface IRouteService
{
    string GetEmailConfirmationRoute(string email,string token);
    string GetResetPasswordRoute(string email, string token);
}