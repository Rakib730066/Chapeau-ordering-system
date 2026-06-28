using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.ViewModels
{
    public class RestaurantOverviewViewModel
    {
        public IEnumerable<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
        public IEnumerable<Order> OpenOrders { get; set; } = new List<Order>();
        public string? EmployeeName { get; set; }
        public string? EmployeeRole { get; set; }
        public bool IsWaiter => EmployeeRole?.Equals("Waiter", StringComparison.OrdinalIgnoreCase) == true;

        public IEnumerable<Order> GetOrdersForTable(int tableId)
            => OpenOrders.Where(o => o.Table?.TableId == tableId);
    }
}
