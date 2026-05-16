using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IBarKitchenRepository
    {
        // Get all orders from database
        List<Order> GetAll();

        // Get single order by OrderId
        Order? GetById(int orderId);

        // Get running orders for Bar or Kitchen (filtered by MenuItemType)
        List<Order> GetRunningOrders(MenuItemType menuItemType);

        // Add new order
        void Add(Order order);

        // Update order
        void Update(Order order);

        // Delete order
        void Delete(int orderId);

        // Update order status
        void UpdateStatus(int orderId, string status);

        // Update all matching items in one order (used by Bar/Kitchen service)
        void UpdateOrderItemsStatusForOrder(
            int orderId,
            MenuItemType menuItemType,
            OrderItemStatus oldStatus,
            OrderItemStatus newStatus);

        // Update one matching order item (used by Bar/Kitchen service)
        void UpdateOrderItemStatus(
            int orderItemId,
            MenuItemType menuItemType,
            OrderItemStatus oldStatus,
            OrderItemStatus newStatus);
    }
}