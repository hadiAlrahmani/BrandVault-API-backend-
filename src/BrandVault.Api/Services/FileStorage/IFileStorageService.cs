namespace BrandVault.Api.Services.FileStorage;

/// <summary>
/// Contract for file storage operations.
///
/// Express/Multer equivalent:
///   interface IFileStorageService {
///     saveFile(file: Express.Multer.File): Promise&lt;{ filePath: string; fileSize: number }&gt;;
///     deleteFile(filePath: string): Promise&lt;void&gt;;
///     getFullPath(relativePath: string): string;
///     validateFile(file: Express.Multer.File): void;
///   }
/// </summary>
public interface IFileStorageService
{
    Task<(string FilePath, long FileSize)> SaveFileAsync(IFormFile file);
    Task DeleteFileAsync(string filePath);
    string GetFullPath(string relativePath);
    void ValidateFile(IFormFile file);
}
