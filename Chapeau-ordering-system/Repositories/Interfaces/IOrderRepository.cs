using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOpenOrders();
      
        // Sprint 2 — Taking Order
        int Add(Order order);                    // inserts order, returns new OrderId
        void AddOrderItem(int orderId, OrderItem item); // inserts one order item
        void DecreaseStock(int menuItemId, int quantity); // decreases stock after save
    }

}
