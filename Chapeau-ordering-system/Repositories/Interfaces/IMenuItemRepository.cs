using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IMenuItemRepository
    {
        List<MenuItem> GetAll();
        List<MenuItem> GetFiltered(MenuItemType? type, CourseType? course,CardType? card);
        MenuItem? GetById(int menuItemId);
        void DecreaseStock(int menuItemId, int quantity);
        void IncreaseStock(int menuItemId, int quantity);
    }
}