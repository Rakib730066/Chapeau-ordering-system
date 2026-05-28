using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;

namespace Chapeau_ordering_system.Services
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository;

        public MenuItemService(IMenuItemRepository menuItemRepository)
        {
            _menuItemRepository = menuItemRepository;
        }

        // Get all menu items
        public List<MenuItem> GetAllMenuItems()
        {
            return _menuItemRepository.GetAll();
        }

        // Get menu items filtered by card type (drinks/food) and course (starters/mains/etc.)
        public List<MenuItem> GetFilteredMenuItems(MenuItemType? type, CourseType? course)
        {
            return _menuItemRepository.GetFiltered(type, course);
        }

        public MenuItem? GetMenuItemById(int menuItemId)
        {
            return _menuItemRepository.GetById(menuItemId);
        }
    }
}