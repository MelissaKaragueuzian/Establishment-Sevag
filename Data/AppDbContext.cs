using Microsoft.EntityFrameworkCore;
using Website_Idea.Models;

namespace Website_Idea.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
    public DbSet<CompanyInfo> CompanyInfos => Set<CompanyInfo>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AboutPage> AboutPages => Set<AboutPage>();

    public Task<List<string>> CategoryNamesAsync() =>
        Categories.AsNoTracking().OrderBy(c => c.Id).Select(c => c.Name).ToListAsync();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(120).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(4000).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(80).IsRequired();
            entity.Property(p => p.Material).HasMaxLength(200);
            entity.Property(p => p.Dimensions).HasMaxLength(200);
            entity.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.Property(i => i.ImagePath).HasMaxLength(400).IsRequired();
        });

        modelBuilder.Entity<GalleryImage>(entity =>
        {
            entity.Property(i => i.ImagePath).HasMaxLength(400).IsRequired();
            entity.Property(i => i.Title).HasMaxLength(160);
            entity.Property(i => i.Category).HasMaxLength(80);
        });

        modelBuilder.Entity<CompanyInfo>(entity =>
        {
            entity.Property(c => c.CompanyName).HasMaxLength(120).IsRequired();
            entity.Property(c => c.Phone).HasMaxLength(40);
            entity.Property(c => c.WhatsApp).HasMaxLength(40);
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.Property(c => c.Address).HasMaxLength(400);
            entity.Property(c => c.GoogleMapsUrl).HasMaxLength(1000);
            entity.Property(c => c.AboutDescription).HasMaxLength(4000);
            entity.Property(c => c.HeroBadge).HasMaxLength(120);
            entity.Property(c => c.HeroTitle).HasMaxLength(160);
            entity.Property(c => c.HeroHighlight).HasMaxLength(120);
            entity.Property(c => c.HeroText).HasMaxLength(1000);
            entity.Property(c => c.HeroFeatures).HasMaxLength(400);
            entity.Property(c => c.HeroImagePath).HasMaxLength(400);
            entity.Property(c => c.CollectionKicker).HasMaxLength(80);
            entity.Property(c => c.CollectionTitle).HasMaxLength(160);
            entity.Property(c => c.CollectionText).HasMaxLength(1000);
        });
    }
}
