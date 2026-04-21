namespace BookOrbit.Application.Common.Interfaces.ImageServices;
public interface IImageServiceBase
{
    Task<Result<string>> UploadImage(Stream imageFile, string fileName);

    void DeleteImage(string fileName);

    Task<byte[]?> GetImage(string fileName);
}