using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.ViewModels
{
    public class MenuViewModel
    {
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public MenuItemType? ActiveType { get; set; }
        public CourseType? ActiveCourse { get; set; }
        public CardType? ActiveCard { get; set; }

        public int TableId { get; set; }
    }
}