using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;

namespace Chapeau_ordering_system.Services
{
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;
        private readonly IOrderRepository _orderRepository;

        public TableService(ITableRepository tableRepository, IOrderRepository orderRepository)
        {
            _tableRepository = tableRepository;
            _orderRepository = orderRepository;
        }

        public IEnumerable<RestaurantTable> GetAllTables() => _tableRepository.GetAllTables();

        public List<RestaurantTable> GetAllTablesOrdered() =>
            _tableRepository.GetAllTables()
                .OrderBy(t => int.TryParse(t.TableNumber.Replace("T", ""), out int n) ? n : 0)
                .ToList();

        public RestaurantTable? GetById(int id) => _tableRepository.GetById(id);

        public void UpdateStatus(int id, TableStatus status, int? currentOrderId = null, string? reservationName = null)
        {
            _tableRepository.UpdateStatus(id, status, currentOrderId, reservationName);
        }

        public void MarkFree(int tableId)
        {
            if (_orderRepository.TableHasUnservedItems(tableId))
                throw new InvalidOperationException("Cannot mark table as free — it still has unserved items.");
            _tableRepository.UpdateStatus(tableId, TableStatus.Free, null);
        }
    }
}
