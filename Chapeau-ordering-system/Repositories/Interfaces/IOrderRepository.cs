using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOpenOrders();
      
       
        int Add(Order order);                   
        void AddOrderItem(int orderId, OrderItem item); 
        void DecreaseStock(int menuItemId, int quantity); 


      
        void UpdateOrderItemQuantity(int orderItemId, int quantity);
        void UpdateOrderItemComment(int orderItemId, string? comment);
        void RemoveOrderItem(int orderItemId);
        void CancelOrder(int orderId);

        List<OrderItem> GetItemsByOrderId(int orderId);
        OrderItem? GetOrderItemById(int orderItemId);
        OrderItem? GetOrderItemByOrderAndMenuItem(int orderId, int menuItemId);
        MenuItem? GetMenuItemById(int menuItemId);
        void IncreaseStock(int menuItemId, int quantity);
    }

}
