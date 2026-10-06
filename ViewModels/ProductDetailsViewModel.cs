using Website_Idea.Models;

namespace Website_Idea.ViewModels;

public class ProductDetailsViewModel
{
    public required Product Product { get; init; }
    public required IReadOnlyList<Product> Related { get; init; }
    public string? WhatsAppUrl { get; init; }
}
