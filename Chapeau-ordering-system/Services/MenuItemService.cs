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

        public List<MenuItem> GetAllMenuItems()          => _menuItemRepository.GetAll();
        public List<MenuItem> GetFilteredMenuItems(MenuItemType? type, CourseType? course, CardType? card)
            => _menuItemRepository.GetFiltered(type, course, card);
        public MenuItem? GetMenuItemById(int menuItemId) => _menuItemRepository.GetById(menuItemId);

        // Management
        public void AddMenuItem(MenuItem item)                   => _menuItemRepository.Add(item);
        public void UpdateMenuItem(MenuItem item)                => _menuItemRepository.Update(item);
        public void SetMenuItemActive(int id, bool isActive)     => _menuItemRepository.SetActive(id, isActive);
        public void UpdateStock(int menuItemId, int newStock)    => _menuItemRepository.UpdateStock(menuItemId, newStock);
    }
}
