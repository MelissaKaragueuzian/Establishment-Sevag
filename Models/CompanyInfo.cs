namespace Website_Idea.Models;

public class CompanyInfo
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = "";
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public string? AboutDescription { get; set; }

    public string? HeroBadge { get; set; }
    public string? HeroTitle { get; set; }
    public string? HeroHighlight { get; set; }
    public string? HeroText { get; set; }
    public string? HeroFeatures { get; set; }
    public string? HeroImagePath { get; set; }
    public int? HeroProductId { get; set; }

    public string? CollectionKicker { get; set; }
    public string? CollectionTitle { get; set; }
    public string? CollectionText { get; set; }

    public const string DefaultHeroBadge = "Handcrafted chandeliers · Bronze · Crystal";
    public const string DefaultHeroTitle = "Illuminate Your Space With";
    public const string DefaultHeroHighlight = "Timeless Elegance";
    public const string DefaultHeroText =
        "Discover handcrafted chandeliers where traditional craftsmanship meets refined design. From bronze details to brilliant crystal, each piece is carefully created to bring warmth, character, and elegance to your space.";
    public const string DefaultHeroFeatures = "Bespoke commissions\nCast bronze\nHand-cut crystal";

    public const string DefaultCollectionKicker = "Curated folio";
    public const string DefaultCollectionTitle = "Signature Atelier Collections";
    public const string DefaultCollectionText =
        "Every luminaire is shaped by hand in our workshop, pairing the weight of cast bronze with the fire of faceted crystal.";

    public string CollectionKickerOrDefault => CollectionKicker ?? DefaultCollectionKicker;
    public string CollectionTitleOrDefault => CollectionTitle ?? DefaultCollectionTitle;
    public string CollectionTextOrDefault => CollectionText ?? DefaultCollectionText;

    public string HeroBadgeOrDefault => HeroBadge ?? DefaultHeroBadge;
    public string HeroTitleOrDefault => HeroTitle ?? DefaultHeroTitle;
    public string HeroHighlightOrDefault => HeroHighlight ?? DefaultHeroHighlight;
    public string HeroTextOrDefault => HeroText ?? DefaultHeroText;

    public IReadOnlyList<string> HeroFeatureList =>
        (HeroFeatures ?? DefaultHeroFeatures).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public string Excerpt(int maxLength = 280)
    {
        var text = (AboutDescription ?? "").Trim();
        if (text.Length <= maxLength)
            return text;

        var cut = text.LastIndexOf(' ', Math.Min(maxLength, text.Length - 1));
        if (cut < maxLength / 2)
            cut = maxLength;

        return string.Concat(text.AsSpan(0, cut).TrimEnd(), "…");
    }

    public const string PlaceholderAddress = "120 Atelier Street";
    public const string DefaultAddress = "EST. Sevag, Lebanon";
    public const string DefaultGoogleMapsUrl =
        "https://www.google.com/maps/place/EST.+Sevag/@33.8867885,35.5527519,17z/data=!3m1!4b1!4m6!3m5!1s0x151f17a1eac8be55:0xc24faec5d80d639!8m2!3d33.8867885!4d35.5527519!16s%2Fg%2F11vc6dn7_c";

    public static CompanyInfo CreateDefault() => new()
    {
        CompanyName = "Atelier Hrag",
        Phone = "+1 000 555 0148",
        WhatsApp = "+1 000 555 0148",
        Email = "studio@example.com",
        Address = DefaultAddress,
        GoogleMapsUrl = DefaultGoogleMapsUrl,
        AboutDescription =
            "Atelier Hrag makes chandeliers, bronze work, crystal, and decorative pieces for houses and public rooms. The work is shaped in the workshop, finished to order, and built to be kept.\n\nDrawings start from the room: the height of the ceiling, the color of the walls, and the way the piece will be seen at night. Bronze is cast and chased in the workshop. Crystal is cut, pinned, and hung until the balance is right."
    };
}
