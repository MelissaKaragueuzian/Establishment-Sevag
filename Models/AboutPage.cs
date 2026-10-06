using System.ComponentModel.DataAnnotations;

namespace Website_Idea.Models;

public class AboutPage
{
    public int Id { get; set; }

    [StringLength(80)]
    [Display(Name = "Small label")]
    public string? Kicker { get; set; }

    [StringLength(160)]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    [StringLength(2000)]
    [Display(Name = "How it started")]
    public string? Intro { get; set; }

    [StringLength(120)]
    [Display(Name = "Step 1 title")]
    public string? Step1Title { get; set; }

    [StringLength(600)]
    [Display(Name = "Step 1 text")]
    public string? Step1Text { get; set; }

    [StringLength(120)]
    [Display(Name = "Step 2 title")]
    public string? Step2Title { get; set; }

    [StringLength(600)]
    [Display(Name = "Step 2 text")]
    public string? Step2Text { get; set; }

    [StringLength(120)]
    [Display(Name = "Step 3 title")]
    public string? Step3Title { get; set; }

    [StringLength(600)]
    [Display(Name = "Step 3 text")]
    public string? Step3Text { get; set; }

    [StringLength(400)]
    [Display(Name = "Quote")]
    public string? Quote { get; set; }

    [StringLength(120)]
    [Display(Name = "Quote by")]
    public string? QuoteAuthor { get; set; }

    [StringLength(400)]
    public string? Image1Path { get; set; }

    [StringLength(80)]
    [Display(Name = "First photo label")]
    public string? Image1Label { get; set; }

    [StringLength(400)]
    public string? Image2Path { get; set; }

    [StringLength(80)]
    [Display(Name = "Second photo label")]
    public string? Image2Label { get; set; }

    [StringLength(40)]
    [Display(Name = "First figure")]
    public string? Stat1Value { get; set; }

    [StringLength(300)]
    [Display(Name = "First figure text")]
    public string? Stat1Text { get; set; }

    [StringLength(40)]
    [Display(Name = "Second figure")]
    public string? Stat2Value { get; set; }

    [StringLength(300)]
    [Display(Name = "Second figure text")]
    public string? Stat2Text { get; set; }

    public IEnumerable<(string Title, string? Text)> Steps()
    {
        if (!string.IsNullOrWhiteSpace(Step1Title)) yield return (Step1Title, Step1Text);
        if (!string.IsNullOrWhiteSpace(Step2Title)) yield return (Step2Title, Step2Text);
        if (!string.IsNullOrWhiteSpace(Step3Title)) yield return (Step3Title, Step3Text);
    }

    public void CopyTextFrom(AboutPage source)
    {
        Kicker = Clean(source.Kicker);
        Title = Clean(source.Title);
        Intro = Clean(source.Intro)?.Replace("\r\n", "\n").Replace('\r', '\n');
        Step1Title = Clean(source.Step1Title);
        Step1Text = Clean(source.Step1Text);
        Step2Title = Clean(source.Step2Title);
        Step2Text = Clean(source.Step2Text);
        Step3Title = Clean(source.Step3Title);
        Step3Text = Clean(source.Step3Text);
        Quote = Clean(source.Quote);
        QuoteAuthor = Clean(source.QuoteAuthor);
        Image1Label = Clean(source.Image1Label);
        Image2Label = Clean(source.Image2Label);
        Stat1Value = Clean(source.Stat1Value);
        Stat1Text = Clean(source.Stat1Text);
        Stat2Value = Clean(source.Stat2Value);
        Stat2Text = Clean(source.Stat2Text);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static AboutPage CreateDefault() => new()
    {
        Kicker = "The métiers d'art",
        Title = "The Art of Haute Éclairage",
        Intro = "What began as a small workshop has grown into an atelier for chandeliers, bronze, and crystal. Every piece is still an unhurried dialogue between molten bronze, the hands that chase it, and the light it will carry.",
        Step1Title = "Hand-Chiseled & Cast Bronze",
        Step1Text = "Each crown, arm, and rosette is cast in bronze, then chased and patinated by hand to reveal depth and warmth.",
        Step2Title = "Faceted Crystal & Light",
        Step2Text = "Crystal drops and prisms are cut and pinned by hand, so they scatter a warm, golden light instead of a harsh glare.",
        Step3Title = "Made to the Scale of the Room",
        Step3Text = "Every commission starts in the room itself: the ceiling height, the weight the ceiling can carry, and the way the piece will be seen at night.",
        Quote = "Lighting is the soul of architecture; crystal and bronze give it breath.",
        QuoteAuthor = "Master artisan",
        Image1Path = "/images/seed/gallery-bronze.svg",
        Image1Label = "Bronze chiseling",
        Image2Path = "/images/seed/gallery-crystal.svg",
        Image2Label = "Faceted pendants",
        Stat1Value = "2,700 K",
        Stat1Text = "A warm, candlelight color temperature that suits antique bronze patina.",
        Stat2Value = "By hand",
        Stat2Text = "Every arm, drop, and fitting is assembled and adjusted in the workshop."
    };
}
