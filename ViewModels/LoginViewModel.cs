using System.ComponentModel.DataAnnotations;

namespace Website_Idea.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter the password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Pin { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
