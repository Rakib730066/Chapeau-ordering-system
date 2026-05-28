using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.ViewModels
{
    public class MenuViewModel
    {
        // The list of menu items to display (already filtered)
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        // The currently active filter values (used to highlight the active button)
        public MenuItemType? ActiveType { get; set; }
        public CourseType? ActiveCourse { get; set; }

        // The table this order is for (passed through from restaurant overview)
        public int TableId { get; set; }
    }
}