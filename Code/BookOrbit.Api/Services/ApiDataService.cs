namespace BookOrbit.Api.Services;
public class ApiDataService(
    IWebHostEnvironment environment) : IApiDataService
{
    public string GetContentRootPath()
    =>
        environment.ContentRootPath;
    

    public string GetWebRootPath()
    =>
        environment.WebRootPath;
    
}