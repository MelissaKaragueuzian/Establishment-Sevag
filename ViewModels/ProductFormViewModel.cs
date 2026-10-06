using System.ComponentModel.DataAnnotations;
using Website_Idea.Models;

namespace Website_Idea.ViewModels;

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Enter a name.")]
    [StringLength(120)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Enter a description.")]
    [StringLength(4000)]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Choose a category.")]
    [StringLength(80)]
    public string Category { get; set; } = ProductCategories.Chandeliers;

    [StringLength(200)]
    public string? Material { get; set; }

    [StringLength(200)]
    public string? Dimensions { get; set; }

    [Display(Name = "Show on the home page")]
    public bool IsFeatured { get; set; }

    public List<ProductImage> ExistingImages { get; set; } = [];
    public List<int> RemoveImageIds { get; set; } = [];
    public List<IFormFile> NewImages { get; set; } = [];
}
