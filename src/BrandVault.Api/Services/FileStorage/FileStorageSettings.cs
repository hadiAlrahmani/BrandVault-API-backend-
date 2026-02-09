namespace BrandVault.Api.Services.FileStorage;

/// <summary>
/// Binds to the "FileStorage" section in appsettings.json.
///
/// Express equivalent:
///   const config = {
///     basePath: "Uploads",
///     maxFileSizeMB: 50,
///     allowedExtensions: [".jpg", ".png", ...]
///   };
/// </summary>
public class FileStorageSettings
{
    public string BasePath { get; set; } = "Uploads";
    public int MaxFileSizeMB { get; set; } = 50;
    public List<string> AllowedExtensions { get; set; } = new();
}
