using System;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Lazybook.Api.Services;

public class FileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly IAmazonS3 _s3Client;
    public FileStorageService(IConfiguration configuration, IAmazonS3 s3Client)
    {
        _configuration = configuration;
        _s3Client = s3Client;
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        // Get the bucket name from configuration
        var bucketName = _configuration["AWS:BucketName"];
        // Use GUIDs to create unique filenames
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        // Save the file to S3
        var fileStream = file.OpenReadStream();

        var transferUtility = new TransferUtility(_s3Client);

        await transferUtility.UploadAsync(fileStream, bucketName, fileName);

        // Return the file URL
        return $"https://{bucketName}.s3.amazonaws.com/{fileName}";
    }
}
