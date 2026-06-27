using System.ComponentModel.DataAnnotations;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.ViewModels
{
    public class EmployeeFormViewModel
    {
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public EmployeeRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        // Only required when creating a new employee
        [StringLength(100, MinimumLength = 4)]
        public string? Password { get; set; }

        public bool IsNew => EmployeeId == 0;
    }
}
