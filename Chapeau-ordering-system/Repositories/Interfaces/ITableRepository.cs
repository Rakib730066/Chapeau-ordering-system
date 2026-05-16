using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface ITableRepository
    {
        IEnumerable<RestaurantTable> GetAllTables();
        RestaurantTable? GetById(int id);
        void UpdateStatus(int id, TableStatus status, int? currentOrderId = null);
    }
}

