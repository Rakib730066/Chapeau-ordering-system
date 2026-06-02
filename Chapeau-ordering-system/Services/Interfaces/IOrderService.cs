using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<Order> GetOpenOrders();
        void MarkItemServed(int orderItemId);
    }
}
