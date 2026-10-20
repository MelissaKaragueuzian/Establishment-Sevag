using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Idea.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = ProductCategories.Chandeliers;
    public string? Material { get; set; }
    public string? Dimensions { get; set; }
    public bool IsFeatured { get; set; }
    public string? VideoPath { get; set; }
    public string? ModelPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductImage> Images { get; set; } = [];

    [NotMapped]
    public List<ProductImage> Photos => Images.OrderBy(i => i.Id).ToList();
}
