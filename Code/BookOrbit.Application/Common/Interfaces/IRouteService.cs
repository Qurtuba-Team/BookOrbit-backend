namespace BookOrbit.Application.Common.Interfaces;
public interface IRouteService
{
   Result<string> GetRouteByName(string? routeName, object? values);
}