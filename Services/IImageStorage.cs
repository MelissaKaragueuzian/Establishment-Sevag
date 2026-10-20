namespace Website_Idea.Services;

public interface IImageStorage
{
    string? Validate(IFormFile file);
    Task<string> SaveAsync(IFormFile file, string folder);
    string? ValidateVideo(IFormFile file);
    Task<string> SaveVideoAsync(IFormFile file, string folder);
    string? ValidateModel(IFormFile file);
    Task<string> SaveModelAsync(IFormFile file, string folder);
    void Delete(string? relativePath);
}
