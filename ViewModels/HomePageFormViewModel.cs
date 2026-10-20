using System.ComponentModel.DataAnnotations;
using Website_Idea.Models;

namespace Website_Idea.ViewModels;

public class HomePageFormViewModel
{
    [StringLength(120)]
    [Display(Name = "Small label above the title")]
    public string? HeroBadge { get; set; }

    [StringLength(160)]
    [Display(Name = "Title")]
    public string? HeroTitle { get; set; }

    [StringLength(120)]
    [Display(Name = "Highlighted words")]
    public string? HeroHighlight { get; set; }

    [StringLength(1000)]
    [Display(Name = "Paragraph")]
    public string? HeroText { get; set; }

    [StringLength(400)]
    [Display(Name = "Features")]
    public string? HeroFeatures { get; set; }

    [Display(Name = "Piece shown in the photo card")]
    public int? HeroProductId { get; set; }

    [Display(Name = "Photo")]
    public IFormFile? HeroImage { get; set; }

    public bool RemoveHeroImage { get; set; }

    public string? CurrentImagePath { get; set; }

    [Display(Name = "Chandelier with the lights off")]
    public IFormFile? HeroLampOff { get; set; }

    [Display(Name = "Chandelier with the lights on")]
    public IFormFile? HeroLampOn { get; set; }

    public bool RemoveHeroLampOff { get; set; }

    public bool RemoveHeroLampOn { get; set; }

    public string CurrentLampOffPath { get; set; } = CompanyInfo.DefaultHeroLampOff;

    public string CurrentLampOnPath { get; set; } = CompanyInfo.DefaultHeroLampOn;

    public bool HasCustomLampOff { get; set; }

    public bool HasCustomLampOn { get; set; }

    public void ShowCurrentImages(CompanyInfo company)
    {
        CurrentImagePath = company.HeroImagePath;
        CurrentLampOffPath = company.HeroLampOffOrDefault;
        CurrentLampOnPath = company.HeroLampOnOrDefault;
        HasCustomLampOff = company.HeroLampOffPath is not null;
        HasCustomLampOn = company.HeroLampOnPath is not null;
    }

    [StringLength(80)]
    [Display(Name = "Small label")]
    public string? CollectionKicker { get; set; }

    [StringLength(160)]
    [Display(Name = "Section title")]
    public string? CollectionTitle { get; set; }

    [StringLength(1000)]
    [Display(Name = "Section text")]
    public string? CollectionText { get; set; }

    public static HomePageFormViewModel From(CompanyInfo company)
    {
        var model = new HomePageFormViewModel
        {
            CollectionKicker = company.CollectionKickerOrDefault,
            CollectionTitle = company.CollectionTitleOrDefault,
            CollectionText = company.CollectionTextOrDefault,
            HeroBadge = company.HeroBadgeOrDefault,
            HeroTitle = company.HeroTitleOrDefault,
            HeroHighlight = company.HeroHighlightOrDefault,
            HeroText = company.HeroTextOrDefault,
            HeroFeatures = string.Join("\n", company.HeroFeatureList),
            HeroProductId = company.HeroProductId
        };
        model.ShowCurrentImages(company);
        return model;
    }
}
