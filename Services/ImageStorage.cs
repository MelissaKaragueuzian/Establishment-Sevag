namespace Website_Idea.Services;

public class ImageStorage(IWebHostEnvironment env) : IImageStorage
{
    private const long MaxBytes = 8 * 1024 * 1024;
    private static readonly HashSet<string> Folders = new(StringComparer.OrdinalIgnoreCase) { "products", "gallery", "home" };
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public string? Validate(IFormFile file)
    {
        if (file.Length <= 0)
            return "Choose an image file.";

        if (file.Length > MaxBytes)
            return "Each image must be 8 MB or smaller.";

        var extension = ExtensionOf(file);
        if (!Extensions.Contains(extension))
            return "Use a JPG, PNG, WEBP, or GIF image.";

        return null;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        if (!Folders.Contains(folder))
            throw new InvalidOperationException("That upload folder is not available.");

        var problem = Validate(file);
        if (problem is not null)
            throw new InvalidOperationException(problem);

        var extension = ExtensionOf(file);
        await using var memory = new MemoryStream();
        await file.CopyToAsync(memory);
        if (!HasImageSignature(memory.ToArray(), extension))
            throw new InvalidOperationException("That file does not look like a valid image.");

        var directory = Path.Combine(env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);
        memory.Position = 0;
        await using var output = File.Create(fullPath);
        await memory.CopyToAsync(output);

        return $"/uploads/{folder}/{fileName}";
    }

    public void Delete(string? relativePath)
    {
        var fullPath = TryMapUpload(relativePath);
        if (fullPath is null || !File.Exists(fullPath))
            return;

        File.Delete(fullPath);
    }

    private string? TryMapUpload(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var normalized = relativePath.Replace('\\', '/');
        const string prefix = "/uploads/";
        if (!normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        var root = Path.GetFullPath(Path.Combine(env.WebRootPath, "uploads"));
        var relative = normalized[prefix.Length..].TrimStart('/');
        var full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = StringComparison.OrdinalIgnoreCase;
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, comparison) && !full.Equals(root, comparison))
            return null;

        return full;
    }

    private static string ExtensionOf(IFormFile file) =>
        Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();

    private static bool HasImageSignature(byte[] data, string extension)
    {
        if (data.Length < 4)
            return false;

        return extension switch
        {
            ".jpg" or ".jpeg" => data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
            ".png" => data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47,
            ".gif" => data.Length >= 4 && data[0] == (byte)'G' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'8',
            ".webp" => data.Length >= 12
                && data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F'
                && data[8] == (byte)'W' && data[9] == (byte)'E' && data[10] == (byte)'B' && data[11] == (byte)'P',
            _ => false
        };
    }
}
