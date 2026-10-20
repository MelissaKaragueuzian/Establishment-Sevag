namespace Website_Idea.Services;

public class ImageStorage(IWebHostEnvironment env) : IImageStorage
{
    private const long MaxBytes = 8 * 1024 * 1024;
    public const long MaxVideoBytes = 100 * 1024 * 1024;
    public const long MaxModelBytes = 60 * 1024 * 1024;
    private static readonly HashSet<string> Folders = new(StringComparer.OrdinalIgnoreCase) { "products", "gallery", "home" };
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm" };

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

    public string? ValidateVideo(IFormFile file)
    {
        if (file.Length <= 0)
            return "Choose a video file.";

        if (file.Length > MaxVideoBytes)
            return "The video must be 100 MB or smaller.";

        if (!VideoExtensions.Contains(ExtensionOf(file)))
            return "Use an MP4 or WEBM video.";

        return null;
    }

    public async Task<string> SaveVideoAsync(IFormFile file, string folder)
    {
        if (ValidateVideo(file) is { } problem)
            throw new InvalidOperationException(problem);

        var extension = ExtensionOf(file);
        return await SaveCheckedAsync(file, folder, extension,
            header => HasVideoSignature(header, extension), "That file does not look like a valid video.");
    }

    public string? ValidateModel(IFormFile file)
    {
        if (file.Length <= 0)
            return "Choose a 3D model file.";

        if (file.Length > MaxModelBytes)
            return "The 3D model must be 60 MB or smaller.";

        if (ExtensionOf(file) != ".glb")
            return "Use a GLB 3D model.";

        return null;
    }

    public async Task<string> SaveModelAsync(IFormFile file, string folder)
    {
        if (ValidateModel(file) is { } problem)
            throw new InvalidOperationException(problem);

        return await SaveCheckedAsync(file, folder, ".glb",
            header => header[0] == (byte)'g' && header[1] == (byte)'l' && header[2] == (byte)'T' && header[3] == (byte)'F',
            "That file does not look like a valid GLB model.");
    }

    public void Delete(string? relativePath)
    {
        var fullPath = TryMapUpload(relativePath);
        if (fullPath is null || !File.Exists(fullPath))
            return;

        File.Delete(fullPath);
    }

    private async Task<string> SaveCheckedAsync(IFormFile file, string folder, string extension, Func<byte[], bool> headerIsValid, string invalidMessage)
    {
        if (!Folders.Contains(folder))
            throw new InvalidOperationException("That upload folder is not available.");

        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var read = 0;
        while (read < header.Length)
        {
            var count = await input.ReadAsync(header.AsMemory(read));
            if (count == 0)
                break;
            read += count;
        }

        if (read < header.Length || !headerIsValid(header))
            throw new InvalidOperationException(invalidMessage);

        var directory = Path.Combine(env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);
        try
        {
            await using var output = File.Create(fullPath);
            await output.WriteAsync(header);
            await input.CopyToAsync(output);
        }
        catch
        {
            File.Delete(fullPath);
            throw;
        }

        return $"/uploads/{folder}/{fileName}";
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

    private static bool HasVideoSignature(byte[] header, string extension) => extension switch
    {
        ".mp4" => header[4] == (byte)'f' && header[5] == (byte)'t' && header[6] == (byte)'y' && header[7] == (byte)'p',
        ".webm" => header[0] == 0x1A && header[1] == 0x45 && header[2] == 0xDF && header[3] == 0xA3,
        _ => false
    };
}
