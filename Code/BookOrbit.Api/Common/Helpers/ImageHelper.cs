namespace BookOrbit.Api.Common.Helpers;

public class ImageHelper()
{
    public static string GetContentType(string extension)
        => extension switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
}
