namespace BrandVault.Api.Services.FileStorage;

using Microsoft.Extensions.Options;
using BrandVault.Api.Common;

/// <summary>
/// Saves uploaded files to disk under Uploads/{yyyy}/{MM}/{guid}_{filename}.
///
/// Express/Multer equivalent:
///   const storage = multer.diskStorage({
///     destination: (req, file, cb) => cb(null, `uploads/${year}/${month}`),
///     filename: (req, file, cb) => cb(null, `${uuid()}_${file.originalname}`),
///   });
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;

    public FileStorageService(IOptions<FileStorageSettings> settings)
    {
        _settings = settings.Value;
    }

    public void ValidateFile(IFormFile file)
    {
        var maxBytes = _settings.MaxFileSizeMB * 1024L * 1024L;
        if (file.Length > maxBytes)
        {
            throw new ApiException(
                $"File size exceeds the maximum of {_settings.MaxFileSizeMB}MB", 400);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) ||
            !_settings.AllowedExtensions.Contains(extension))
        {
            throw new ApiException(
                $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", _settings.AllowedExtensions)}", 400);
        }
    }

    public async Task<(string FilePath, long FileSize)> SaveFileAsync(IFormFile file)
    {
        var now = DateTime.UtcNow;
        var relativeDir = Path.Combine(now.Year.ToString(), now.Month.ToString("D2"));
        var absoluteDir = Path.Combine(_settings.BasePath, relativeDir);

        Directory.CreateDirectory(absoluteDir);

        // Path.GetFileName strips directory components — prevents path traversal
        var safeFileName = Path.GetFileName(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
        var relativePath = Path.Combine(relativeDir, uniqueFileName);
        var absolutePath = Path.Combine(_settings.BasePath, relativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return (relativePath, file.Length);
    }

    public Task DeleteFileAsync(string filePath)
    {
        var absolutePath = GetFullPath(filePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
        return Task.CompletedTask;
    }

    public string GetFullPath(string relativePath)
    {
        return Path.Combine(_settings.BasePath, relativePath);
    }
}
