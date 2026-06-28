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

        public int  TableId        { get; set; }
        public bool HasActiveOrder { get; set; }

        public IEnumerable<IGrouping<MenuItemType, MenuItem>> ItemsByType =>
            MenuItems.GroupBy(i => i.Type).OrderBy(g => g.Key);

        public IEnumerable<CourseType> DisplayCourses =>
            Enum.GetValues<CourseType>().Where(c => c != CourseType.None);

        public bool ShowCourseFilter =>
            ActiveType == null || ActiveType == MenuItemType.Food;

        // Food items pre-grouped by course, filtered and ordered — keeps views logic-free
        public IEnumerable<IGrouping<CourseType, MenuItem>> FoodItemsByCourse =>
            MenuItems
                .Where(i => i.Type == MenuItemType.Food)
                .GroupBy(i => i.Course)
                .Where(g => DisplayCourses.Contains(g.Key))
                .OrderBy(g => g.Key);
    }
}