using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Models
{
    public class RestaurantTable
    {
        public int TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int NumberOfSeats { get; set; }
        public TableStatus Status { get; set; }
        public int? CurrentOrderId { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? Area { get; set; }
        public bool IsActive { get; set; } = true;

        public bool IsFree => Status == TableStatus.Free;
        public int? MinutesSinceOccupied => OccupiedSince.HasValue
            ? Infrastructure.AppClock.MinutesSince(OccupiedSince.Value)
            : null;
    }
}
