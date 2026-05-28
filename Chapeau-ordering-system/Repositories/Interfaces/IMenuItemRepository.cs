using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IMenuItemRepository
    {
        // Get all menu items
        List<MenuItem> GetAll();

        // Get menu items filtered by type and/or course (filtering done in SQL)
        List<MenuItem> GetFiltered(MenuItemType? type, CourseType? course);

        // Get single menu item by id
        MenuItem? GetById(int menuItemId);
    }
}