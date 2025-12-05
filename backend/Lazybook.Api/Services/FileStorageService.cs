using System;

namespace Lazybook.Api.Services;

public class FileStorageService
{
    private readonly IWebHostEnvironment _env;
    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        // Use GUIDs to create unique filenames
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        // Path to "uploads" dir
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");

        // Ensure directory exists
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var filePath = Path.Combine(uploadsDir, fileName);

        // Save the file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return the path to file
        return $"/uploads/{fileName}";
    }
}
