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

        public IEnumerable<OrderItem> ActiveItems =>
            OrderItems.Where(i => i.Status != OrderItemStatus.Cancelled);

        public DateTime FinishedAt =>
            OrderItems.Any(i => i.ReadyAt != default)
                ? OrderItems.Where(i => i.ReadyAt != default).Max(i => i.ReadyAt)
                : OrderTime;

        public int MinutesSinceOrder    => Infrastructure.AppClock.MinutesSince(OrderTime);
        public int MinutesSinceFinished => Infrastructure.AppClock.MinutesSince(FinishedAt);

        public List<OrderItem> GetItemsByCourse(Enums.CourseType course) =>
            OrderItems.Where(i => i.MenuItem?.Course == course).ToList();
    }
}
