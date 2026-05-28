using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IMenuItemService
    {
        // Get all menu items
        List<MenuItem> GetAllMenuItems();

        // Get menu items filtered by card type and/or course
        List<MenuItem> GetFilteredMenuItems(MenuItemType? type, CourseType? course);
    }
}