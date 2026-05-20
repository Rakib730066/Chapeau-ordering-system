using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;

namespace Chapeau_ordering_system.Services
{
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;

        public TableService(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public IEnumerable<RestaurantTable> GetAllTables() => _tableRepository.GetAllTables();

        public RestaurantTable? GetById(int id) => _tableRepository.GetById(id);

        public void UpdateStatus(int id, TableStatus status, int? currentOrderId = null)
        {
            _tableRepository.UpdateStatus(id, status, currentOrderId);
        }
    }
}
