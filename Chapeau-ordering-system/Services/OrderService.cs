using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;

namespace Chapeau_ordering_system.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITableRepository _tableRepository;

        public OrderService(IOrderRepository orderRepository,
                            ITableRepository tableRepository)
        {
            _orderRepository = orderRepository;
            _tableRepository = tableRepository;
        }

        public IEnumerable<Order> GetOpenOrders()
        {
            return _orderRepository.GetOpenOrders();
        }

        // Create a new order in DB, mark table as Occupied, return new OrderId
        public int StartOrder(int tableId, int employeeId)
        {
            RestaurantTable? table = _tableRepository.GetById(tableId);
            if (table == null)
                throw new Exception("Table not found.");

            Order order = new Order
            {
                Table = table,
                Employee = new Employee { EmployeeId = employeeId },
                OrderTime = DateTime.Now,
                Status = OrderStatus.Open
            };

            int newOrderId = _orderRepository.Add(order);
            _tableRepository.UpdateStatus(tableId, TableStatus.Occupied, newOrderId);
            return newOrderId;
        }

        // Save all order items to DB and decrease stock for each item
        public void SaveOrder(int orderId, List<OrderItem> items)
        {
            foreach (OrderItem item in items)
            {
                _orderRepository.AddOrderItem(orderId, item);
                _orderRepository.DecreaseStock(item.MenuItem!.MenuItemId, item.Quantity);
            }
        }
    }
}