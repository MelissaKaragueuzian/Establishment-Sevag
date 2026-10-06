using Website_Idea.Models;

namespace Website_Idea.ViewModels;

public class HomeViewModel
{
    public required CompanyInfo Company { get; init; }
    public required IReadOnlyList<Product> Featured { get; init; }
    public Product? Spotlight { get; init; }
    public string? HeroImage { get; init; }
}
