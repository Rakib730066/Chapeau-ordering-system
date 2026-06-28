using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<Order> GetOpenOrders();

        int StartOrder(int tableId, int employeeId);

        void AddItemToOrder(int orderId, int menuItemId);

        void SaveOrder(int orderId);

        void IncreaseItemQuantity(int orderItemId);

        void DecreaseItemQuantity(int orderItemId);

        void UpdateItemComment(int orderItemId, string? comment);

        void RemoveItem(int orderItemId);

        void CancelOrder(int orderId, int tableId);

        List<OrderItem> GetItemsByOrderId(int orderId);
        List<OrderItem> GetSentItemsByTableId(int tableId);

        Order? GetOrderById(int orderId);

        Order? GetOrderByTableId(int tableId);

        bool TableHasUnservedItems(int tableId);

        void MarkItemServed(int orderItemId);

        string GetItemNameById(int menuItemId);
        string GetItemNameByOrderItemId(int orderItemId);
        OrderItem? GetOrderItemById(int orderItemId);
    }
}