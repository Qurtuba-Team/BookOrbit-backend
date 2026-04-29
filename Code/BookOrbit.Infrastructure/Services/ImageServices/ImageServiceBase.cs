
namespace BookOrbit.Infrastructure.Services.ImageServices
{
    public abstract class ImageServiceBase(
    ILogger logger) : IImageServiceBase
    {
        protected abstract string UploadFolderPath { get; }
        protected abstract string DefaultImageName { get; }

        protected static readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

        static private bool IsValidImageExtension(string extension)
        {
            return allowedExtensions.Contains(extension);
        }
        public async Task<Result<string>> UploadImage(Stream imageFile, string fileName)
        {
            if (imageFile == null || imageFile.Length == 0 || string.IsNullOrWhiteSpace(fileName))
                return Error.Validation("Image.Required", "No Image Was Uploaded");

            var extension = Path.GetExtension(fileName).ToLower();

            if (!IsValidImageExtension(extension))
                return Error.Validation("Image.InvalidType", "Only image files are allowed");

            if (imageFile.Length > 2 * 1024 * 1024)
                return Error.Validation("Image.TooLarge", "Max size is 2MB");

            fileName = $"{Guid.NewGuid()}{extension}";

            if (!Directory.Exists(UploadFolderPath))
                Directory.CreateDirectory(UploadFolderPath);

            var filePath = Path.Combine(UploadFolderPath, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error While Uploading Image Path : {path}", filePath);
                return Error.Failure("ImageUploadFailure", "Failure While Uploading Image");
            }

            return fileName;
        }
        public void DeleteImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            try
            {
                var filePath = Path.Combine(UploadFolderPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error While Deleting Image: {fileName}", fileName);
            }
        }
        private async Task<Result<byte[]>> LoadImage(string fileName)
        {
            var path = Path.Combine(UploadFolderPath, fileName);

            logger.LogCritical("Trying To Load Image With Name : {path}", path);


            if (!System.IO.File.Exists(path))
            {
                logger.LogWarning("This Image Was not Found , ImageName : {fileName}", fileName);
                return Error.NotFound("Image.NotFound", "This Image Was Not Found ");
            }

            try
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                return bytes;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error While Loading Image: {fileName}", fileName);
                return Error.Failure("Image.Failure", "Unexcpected Error While Loading Image");
            }
        }
        public async Task<byte[]?> GetImage(string fileName)
        {
            //Sanitize FileName
            fileName = Path.GetFileName(fileName);

            //Load Image
            var result = await LoadImage(fileName);

            if (result.IsSuccess)
                return result.Value;

            //Load Default
            var defaultImageResult = await LoadImage(DefaultImageName);

            if (defaultImageResult.IsSuccess)
                return defaultImageResult.Value;

            return null;
        }
    }
}
