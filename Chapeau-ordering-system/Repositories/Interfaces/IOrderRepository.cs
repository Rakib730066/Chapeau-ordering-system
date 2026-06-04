using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        // READ
        IEnumerable<Order> GetOpenOrders();
        Order? GetOrderById(int orderId);
        Order? GetOrderByTableId(int tableId);
        List<OrderItem> GetItemsByOrderId(int orderId);
        OrderItem? GetOrderItemById(int orderItemId);
        OrderItem? GetOrderItemByOrderAndMenuItem(int orderId, int menuItemId);
        MenuItem? GetMenuItemById(int menuItemId);

        // CREATE
        int Add(Order order);
        void AddOrderItem(int orderId, OrderItem item);

        // UPDATE ITEMS
        void UpdateOrderItemQuantity(int orderItemId, int quantity);
        void UpdateOrderItemComment(int orderItemId, string? comment);
        void RemoveOrderItem(int orderItemId);

        // ORDER STATUS
        void UpdateOrderStatus(int orderId, OrderStatus status);
        void CancelOrder(int orderId);

        // ORDER ITEM STATUS
        void UpdateOrderItemStatus(int orderItemId, OrderItemStatus status);
        bool TableHasUnservedItems(int tableId);

        // STOCK
        void DecreaseStock(int menuItemId, int quantity);
        void IncreaseStock(int menuItemId, int quantity);
    }
}