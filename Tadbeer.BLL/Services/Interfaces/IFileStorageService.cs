using Microsoft.AspNetCore.Http;

namespace Tadbeer.BLL.Services.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves an uploaded file and returns the relative path/URL
    /// </summary>
    Task<string> SaveFileAsync(IFormFile file, string subfolder, Guid userId);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Gets the full server path from a relative path
    /// </summary>
    string GetFullPath(string relativePath);

    /// <summary>
    /// Validates file extension and size
    /// </summary>
    bool ValidateFile(IFormFile file, string[] allowedExtensions, long maxSizeBytes);
}

