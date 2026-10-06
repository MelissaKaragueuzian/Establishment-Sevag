using Website_Idea.Models;

namespace Website_Idea.ViewModels;

public class AboutViewModel
{
    public required CompanyInfo Company { get; init; }
    public required AboutPage Page { get; init; }
}
