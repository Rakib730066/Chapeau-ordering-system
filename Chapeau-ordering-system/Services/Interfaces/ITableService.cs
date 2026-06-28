using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface ITableService
    {
        IEnumerable<RestaurantTable> GetAllTables();
        RestaurantTable? GetById(int id);
        void UpdateStatus(int id, TableStatus status, int? currentOrderId = null, string? reservationName = null);
        void MarkFree(int tableId);
    }
}
