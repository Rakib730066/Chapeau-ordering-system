using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public RestaurantTable? Table { get; set; }
        public Employee? Employee { get; set; }
        public DateTime OrderTime { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public bool HasUnservedItems =>
            OrderItems.Any(i => i.Status != OrderItemStatus.Served
                             && i.Status != OrderItemStatus.Cancelled);
    }
}
