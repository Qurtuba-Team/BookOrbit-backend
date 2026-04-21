namespace BookOrbit.Application.Common.Interfaces;
public interface IRouteService
{
    string GetEmailConfirmationRoute(string email,string token);
    string GetResetPasswordRoute(string email, string token);
    string GetBookCoverImageRoute();//Return Base
    string GetBookCoverImageRoute(string fileName);//Return Base + [FileName]

}