using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IMenuItemService
    {
        List<MenuItem> GetAllMenuItems();
        List<MenuItem> GetFilteredMenuItems(MenuItemType? type, CourseType? course, CardType? card);
        MenuItem? GetMenuItemById(int menuItemId);

        // Management
        void AddMenuItem(MenuItem item);
        void UpdateMenuItem(MenuItem item);
        void SetMenuItemActive(int menuItemId, bool isActive);
        void UpdateStock(int menuItemId, int newStock);
    }
}
