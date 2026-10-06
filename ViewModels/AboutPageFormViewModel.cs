using Website_Idea.Models;

namespace Website_Idea.ViewModels;

public class AboutPageFormViewModel
{
    public AboutPage Content { get; set; } = new();
    public IFormFile? Image1 { get; set; }
    public IFormFile? Image2 { get; set; }
    public bool RemoveImage1 { get; set; }
    public bool RemoveImage2 { get; set; }
}
