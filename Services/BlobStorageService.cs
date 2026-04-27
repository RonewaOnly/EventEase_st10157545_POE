using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace EventEase_st10157545_POE.Services
{
    /// <summary>
    /// Handles image uploads and deletions against Azure Blob Storage.
    /// Container names: "venue-images" and "event-images".
    /// Call UploadImageAsync to save a file and get back the public URL.
    /// Call DeleteImageAsync to remove an image when a venue or event is deleted.
    /// </summary>
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string VenueContainer = "venue-images";
        private const string EventContainer = "event-images";
        public BlobStorageService(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("AzureBlobStorage")
                ?? config["AzureBlobStorage"]
                ?? throw new InvalidOperationException(
                    "AzureBlobStorage connection string is not configured. " +
                    "Add it to App Service Configuration or appsettings.Development.json.");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
        // ── Upload ────────────────────────────────────────────────────────────
        /// <summary>
        /// Uploads an image file to the specified container and returns its public URL.
        /// Accepts JPEG, PNG, GIF, and WEBP only. Max size 5 MB enforced by the caller.
        /// </summary>
        public async Task<string?> UploadImageAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0) return null;
            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new InvalidOperationException("Only JPEG, PNG, GIF, and WEBP images are accepted.");
            // Validate file size — 5 MB max
            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("Image must be smaller than 5 MB.");
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
            // Generate a unique filename to prevent overwrite collisions
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blobName = $"{Guid.NewGuid()}{extension}";
            var blobClient = container.GetBlobClient(blobName);
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });
            return blobClient.Uri.ToString();
        }
        // ── Upload helpers by entity type 
        public Task<string?> UploadVenueImageAsync(IFormFile file)
            => UploadImageAsync(file, VenueContainer);
        public Task<string?> UploadEventImageAsync(IFormFile file)
            => UploadImageAsync(file, EventContainer);
        // ── Delete 
        /// <summary>
        /// Deletes a blob by its full URL. Safe to call with null or non-blob URLs.
        /// </summary>
        public async Task DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.TrimStart('/').Split('/', 2);
                if (segments.Length < 2) return;
                var containerName = segments[0];
                var blobName = segments[1];
                var container = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = container.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // Swallow — if the blob doesn't exist or URL is invalid, do nothing
            }
        }
        // ── Utility 
        public static bool IsValidImageFile(IFormFile? file)
        {
            if (file == null || file.Length == 0) return false;
            var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            return allowed.Contains(file.ContentType.ToLower()) && file.Length <= 5 * 1024 * 1024;
        }
    }
}
