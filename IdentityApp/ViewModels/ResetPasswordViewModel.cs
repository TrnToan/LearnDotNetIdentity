using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace IdentityApp.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]

    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]

    public string Password { get; set; }
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("Password", ErrorMessage = "Passwords don't match!")]
    public string ConfirmPassword { get; set; }
    public string Token { get; set; }
}
