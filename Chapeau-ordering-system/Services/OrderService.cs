using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;

namespace Chapeau_ordering_system.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public IEnumerable<Order> GetOpenOrders()
        {
            return _orderRepository.GetOpenOrders();
        }

        public bool TableHasUnservedItems(int tableId)
        {
            return GetOpenOrders()
                .Where(o => o.Table?.TableId == tableId)
                .Any(o => o.HasUnservedItems);
        }

        public void MarkItemServed(int orderItemId)
        {
            _orderRepository.UpdateOrderItemStatus(orderItemId, OrderItemStatus.Served);
        }
    }
}
