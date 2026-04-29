
namespace BookOrbit.Infrastructure.Services.ImageServices;
public class StudentImageService(
    ILogger<StudentImageService> logger,
    IApiDataService apiDataService) : ImageServiceBase(logger), IStudentImageService
{
    protected override string UploadFolderPath => Path.Combine(apiDataService.GetContentRootPath(), "uploads", "students");

    protected override string DefaultImageName => "DefaultStudentImage.png";
}
