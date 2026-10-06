using System.ComponentModel.DataAnnotations;

namespace Website_Idea.ViewModels;

public class GalleryUploadViewModel
{
    public IFormFile? Image { get; set; }

    [StringLength(160)]
    public string? Title { get; set; }

    [StringLength(80)]
    public string? Category { get; set; }
}
