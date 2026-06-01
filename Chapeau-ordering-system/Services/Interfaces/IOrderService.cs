using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<Order> GetOpenOrders();

        int StartOrder(int tableId, int employeeId);

        void AddItemToOrder(int orderId, int menuItemId);

        void SaveOrder(int orderId, List<OrderItem> items);

        void IncreaseItemQuantity(int orderItemId, int currentQuantity);

        void DecreaseItemQuantity(int orderItemId, int currentQuantity);

        void UpdateItemComment(int orderItemId, string? comment);

        void RemoveItem(int orderItemId);

        void CancelOrder(int orderId, int tableId);

        List<OrderItem> GetItemsByOrderId(int orderId);

        Order? GetOrderById(int orderId);

        Order?GetOrderByTableId(int tableId);

    }
}