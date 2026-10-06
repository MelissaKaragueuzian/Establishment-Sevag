namespace Website_Idea.Models;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImagePath { get; set; } = "";
    public Product Product { get; set; } = null!;
}
