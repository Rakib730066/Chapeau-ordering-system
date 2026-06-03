using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IMenuItemService
    {
      
        List<MenuItem> GetAllMenuItems();
        List<MenuItem> GetFilteredMenuItems(MenuItemType? type, CourseType? course, CardType? card);
        MenuItem? GetMenuItemById(int menuItemId);
    }
}