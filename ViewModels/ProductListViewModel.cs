using Website_Idea.Models;

namespace Website_Idea.ViewModels;

public class ProductListViewModel
{
    public string? Category { get; init; }
    public required IReadOnlyList<Product> Products { get; init; }
}
