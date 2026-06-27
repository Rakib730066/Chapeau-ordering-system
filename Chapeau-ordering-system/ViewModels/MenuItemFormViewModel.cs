using System.ComponentModel.DataAnnotations;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.ViewModels
{
    public class MenuItemFormViewModel
    {
        public int MenuItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 9999.99)]
        public decimal Price { get; set; }

        [Required]
        public MenuItemType Type { get; set; }

        [Required]
        public CourseType Course { get; set; }

        [Required]
        public CardType Card { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal VatRate { get; set; }

        [Required]
        [Range(0, 9999)]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
