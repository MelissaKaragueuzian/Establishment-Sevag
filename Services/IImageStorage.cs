namespace Website_Idea.Services;

public interface IImageStorage
{
    string? Validate(IFormFile file);
    Task<string> SaveAsync(IFormFile file, string folder);
    void Delete(string? relativePath);
}
