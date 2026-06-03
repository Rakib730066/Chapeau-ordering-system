using Chapeau_ordering_system.Models.Enums;
namespace Chapeau_ordering_system.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public MenuItemType Type { get; set; }
        public CourseType Course { get; set; }
        public CardType Card { get; set; }
        public decimal VatRate { get; set; }
        public int Stock { get; set; }
        public bool IsOutOfStock => Stock <= 0;
        public bool IsLowStock => Stock > 0 && Stock <= 10;
    }
}