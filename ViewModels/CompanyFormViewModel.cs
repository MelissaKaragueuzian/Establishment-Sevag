using System.ComponentModel.DataAnnotations;

namespace Website_Idea.ViewModels;

public class CompanyFormViewModel
{
    [Required(ErrorMessage = "Enter the company name.")]
    [StringLength(120)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = "";

    [StringLength(40)]
    [Display(Name = "Phone number")]
    public string? Phone { get; set; }

    [StringLength(40)]
    [Display(Name = "WhatsApp number")]
    public string? WhatsApp { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(400)]
    public string? Address { get; set; }

    [StringLength(1000)]
    [Display(Name = "Google Maps link")]
    public string? GoogleMapsUrl { get; set; }

    [StringLength(4000)]
    [Display(Name = "About the atelier")]
    public string? AboutDescription { get; set; }

    public static CompanyFormViewModel From(Models.CompanyInfo company) => new()
    {
        CompanyName = company.CompanyName,
        Phone = company.Phone,
        WhatsApp = company.WhatsApp,
        Email = company.Email,
        Address = company.Address,
        GoogleMapsUrl = company.GoogleMapsUrl,
        AboutDescription = company.AboutDescription
    };
}
