using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<Order> GetOpenOrders();

        // Sprint 2 — Taking Order
        int StartOrder(int tableId, int employeeId);
        void SaveOrder(int orderId, List<OrderItem> items);
    }
}