using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOpenOrders();
        void UpdateOrderItemStatus(int orderItemId, OrderItemStatus status);
    }
}
