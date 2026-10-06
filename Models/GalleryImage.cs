namespace Website_Idea.Models;

public class GalleryImage
{
    public int Id { get; set; }
    public string ImagePath { get; set; } = "";
    public string? Title { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
}
