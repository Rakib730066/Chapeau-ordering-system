using System.ComponentModel.DataAnnotations;

namespace Chapeau_ordering_system.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password / PIN")]
        public string Password { get; set; } = string.Empty;
    }
}