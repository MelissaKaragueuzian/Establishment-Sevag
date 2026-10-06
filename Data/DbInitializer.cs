using Microsoft.EntityFrameworkCore;
using Website_Idea.Models;

namespace Website_Idea.Data;

public static class DbInitializer
{
    private const string Seed = "/images/seed";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var existing = await db.CompanyInfos.FirstOrDefaultAsync();
        if (existing is not null)
        {
            if (existing.Address == CompanyInfo.PlaceholderAddress)
            {
                existing.Address = CompanyInfo.DefaultAddress;
                existing.GoogleMapsUrl = CompanyInfo.DefaultGoogleMapsUrl;
                await db.SaveChangesAsync();
            }
            return;
        }

        db.CompanyInfos.Add(CompanyInfo.CreateDefault());
        db.Products.AddRange(SampleProducts());
        db.GalleryImages.AddRange(SampleGallery());
        await db.SaveChangesAsync();
    }

    private static DateTime On(int day) => new(2026, 1, day, 10, 0, 0, DateTimeKind.Utc);

    private static Product Piece(
        string name,
        string category,
        string description,
        string material,
        string dimensions,
        bool featured,
        int day,
        params string[] images) => new()
    {
        Name = name,
        Category = category,
        Description = description,
        Material = material,
        Dimensions = dimensions,
        IsFeatured = featured,
        CreatedAt = On(day),
        Images = images.Select(path => new ProductImage { ImagePath = path }).ToList()
    };

    private static IEnumerable<Product> SampleProducts()
    {
        yield return Piece(
            "Cascade Chandelier",
            ProductCategories.Chandeliers,
            "A tiered chandelier of cast bronze arms and long crystal drops, made for a high ceiling. The armature is balanced by hand so the light falls in a quiet circle.",
            "Cast bronze, hand-cut crystal",
            "110 cm diameter, 140 cm drop",
            featured: true,
            day: 4,
            $"{Seed}/cascade.svg",
            $"{Seed}/cascade-detail.svg");

        yield return Piece(
            "Empire Crown",
            ProductCategories.Chandeliers,
            "A shallow crown of gilded bronze with ropes of crystal. It sits closer to the ceiling and suits a dining room or a formal landing.",
            "Gilded bronze, crystal beads",
            "80 cm diameter, 95 cm drop",
            featured: true,
            day: 5,
            $"{Seed}/empire.svg");

        yield return Piece(
            "Palmette Sconce",
            ProductCategories.BronzeWorks,
            "A single bronze arm with a palmette backplate. Use it in a pair along a corridor or beside a mirror.",
            "Cast bronze",
            "18 cm wide, 42 cm high, 22 cm projection",
            featured: true,
            day: 6,
            $"{Seed}/sconce.svg");

        yield return Piece(
            "Medici Urn",
            ProductCategories.BronzeWorks,
            "A classical urn in cast bronze, finished with a warm brown patina. It is meant for a table, a niche, or a stone floor.",
            "Cast bronze with a warm patina",
            "36 cm diameter, 48 cm high",
            featured: false,
            day: 3,
            $"{Seed}/urn.svg");

        yield return Piece(
            "Prism Obelisk",
            ProductCategories.Crystal,
            "A tall prism of cut crystal on a small bronze base. Set it where daylight can reach it.",
            "Cut crystal, bronze base",
            "12 cm square, 46 cm high",
            featured: true,
            day: 7,
            $"{Seed}/obelisk.svg");

        yield return Piece(
            "Faceted Bowl",
            ProductCategories.Crystal,
            "A low bowl cut with broad facets. It holds light on a table without asking for flowers.",
            "Cut crystal",
            "28 cm diameter, 14 cm high",
            featured: false,
            day: 2,
            $"{Seed}/bowl.svg");

        yield return Piece(
            "Lunette Mirror",
            ProductCategories.DecorativePieces,
            "An arched mirror in a slender bronze frame, with a small crest at the crown. The glass is beveled by hand.",
            "Bronze frame, mirrored glass",
            "70 cm wide, 110 cm high",
            featured: true,
            day: 8,
            $"{Seed}/mirror.svg");

        yield return Piece(
            "Laurel Finial",
            ProductCategories.DecorativePieces,
            "A laurel finial cast in bronze, for a stair newel, a gate, or the top of a frame.",
            "Cast bronze",
            "9 cm wide, 32 cm high",
            featured: false,
            day: 1,
            $"{Seed}/finial.svg");

        yield return Piece(
            "Grand Stair Luminaire",
            ProductCategories.CustomWorks,
            "A long luminaire drawn for a stair void. The bronze spine is made to the height of the hall, then dressed with crystal in the workshop.",
            "Bronze armature, crystal drops",
            "Made to the stair, typically 3 to 6 meters",
            featured: true,
            day: 9,
            $"{Seed}/stair.svg",
            $"{Seed}/stair-detail.svg");
    }

    private static IEnumerable<GalleryImage> SampleGallery() =>
    [
        new() { Title = "Evening hall", Category = ProductCategories.Chandeliers, ImagePath = $"{Seed}/gallery-hall.svg", CreatedAt = On(10) },
        new() { Title = "Table bronze", Category = ProductCategories.BronzeWorks, ImagePath = $"{Seed}/gallery-table.svg", CreatedAt = On(11) },
        new() { Title = "Paired sconces", Category = ProductCategories.BronzeWorks, ImagePath = $"{Seed}/gallery-wall.svg", CreatedAt = On(12) },
        new() { Title = "Vessel study", Category = ProductCategories.BronzeWorks, ImagePath = $"{Seed}/gallery-bronze.svg", CreatedAt = On(13) },
        new() { Title = "Crystal cluster", Category = ProductCategories.Crystal, ImagePath = $"{Seed}/gallery-crystal.svg", CreatedAt = On(14) },
        new() { Title = "Night lighting", Category = ProductCategories.Chandeliers, ImagePath = $"{Seed}/gallery-night.svg", CreatedAt = On(15) }
    ];
}
