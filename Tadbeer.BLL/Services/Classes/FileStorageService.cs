using Microsoft.AspNetCore.Http;
using Tadbeer.BLL.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Tadbeer.BLL.Services.Classes;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadRoot;

    public FileStorageService(IConfiguration configuration)
    {
        var configuredRoot = configuration["FileStorage:UploadDirectory"] ?? "wwwroot/uploads";
        _uploadRoot = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(Directory.GetCurrentDirectory(), configuredRoot);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subfolder, Guid userId)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File cannot be empty.", nameof(file));
        }

        var hasUserScope = userId != Guid.Empty;
        var uploadDir = hasUserScope
            ? Path.Combine(_uploadRoot, subfolder, userId.ToString())
            : Path.Combine(_uploadRoot, subfolder);
        Directory.CreateDirectory(uploadDir);

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path for database storage
        return hasUserScope
            ? Path.Combine("uploads", subfolder, userId.ToString(), fileName).Replace("\\", "/")
            : Path.Combine("uploads", subfolder, fileName).Replace("\\", "/");
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var fullPath = GetFullPath(filePath);
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath));
        }
    }

    public string GetFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var normalized = relativePath.Replace("\\", "/").Trim();

        // If the DB has a full URL, keep only its path segment.
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri))
        {
            normalized = absoluteUri.AbsolutePath;
        }

        // If this is already an absolute file path, use it as-is.
        if (Path.IsPathRooted(normalized) && !normalized.StartsWith("/"))
        {
            return normalized;
        }

        normalized = normalized.TrimStart('/');
        if (normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["uploads/".Length..];
        }

        return Path.Combine(_uploadRoot, normalized.Replace("/", Path.DirectorySeparatorChar.ToString()));
    }

    public bool ValidateFile(IFormFile file, string[] allowedExtensions, long maxSizeBytes)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }

        if (file.Length > maxSizeBytes)
        {
            return false;
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(fileExtension);
    }
}

