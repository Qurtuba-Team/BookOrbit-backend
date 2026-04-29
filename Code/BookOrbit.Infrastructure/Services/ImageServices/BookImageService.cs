
namespace BookOrbit.Infrastructure.Services.ImageServices;
public class BookImageService(
    ILogger<BookImageService> logger,
    IApiDataService apiDataService) : ImageServiceBase(logger), IBookImageService
{
    protected override string UploadFolderPath => Path.Combine(apiDataService.GetWebRootPath(), "uploads", "books");

    protected override string DefaultImageName => "DefaultBookCoverImage.png";
}
