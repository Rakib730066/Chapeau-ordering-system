using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IBarKitchenRepository
    {
        // Get running orders for Bar or Kitchen (filtered by MenuItemType)
        List<Order> GetRunningOrders(MenuItemType menuItemType);

        // Get finished orders today for Bar or Kitchen (filtered by MenuItemType)
        List<Order> GetFinishedOrdersToday(MenuItemType menuItemType);

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

        // Update all items in a course (Kitchen only, Food items)
        void UpdateCourseStatus(
            int orderId,
            CourseType courseType,
            OrderItemStatus oldStatus,
            OrderItemStatus newStatus);

        // Mark all items in order as ready to be served (regardless of current status)
        void UpdateAllOrderItemsToReady(int orderId, MenuItemType menuItemType);
    }
}