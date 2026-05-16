using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public MenuItem? MenuItem { get; set; }
        public int Quantity { get; set; }
        public string? Comment { get; set; }
        public OrderItemStatus Status { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime ReadyAt { get; internal set; }
        public DateTime StartedAt { get; internal set; }
    }
}
