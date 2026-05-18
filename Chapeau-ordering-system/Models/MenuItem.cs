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
    }
}
