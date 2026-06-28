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

        public bool IsFree       => Status == TableStatus.Free;
        public bool IsOccupied   => Status == TableStatus.Occupied;
        public bool IsReserved   => Status == TableStatus.Reserved;
        public bool IsCleaning   => Status == TableStatus.Cleaning;
        public bool IsLongWait   => MinutesSinceOccupied > 90;

        public int? MinutesSinceOccupied => OccupiedSince.HasValue
            ? Infrastructure.AppClock.MinutesSince(OccupiedSince.Value)
            : null;

        public string StatusBadgeClass => Status switch
        {
            TableStatus.Free      => "bg-success",
            TableStatus.Occupied  => "bg-danger",
            TableStatus.Reserved  => "bg-warning text-dark",
            TableStatus.Cleaning  => "bg-info text-dark",
            _                     => "bg-secondary"
        };

        public string CardBorderClass => IsLongWait ? "border-warning" : Status switch
        {
            TableStatus.Free     => "border-success",
            TableStatus.Occupied => "border-danger",
            TableStatus.Reserved => "border-warning",
            TableStatus.Cleaning => "border-info",
            _                    => "border-secondary"
        };

        public string CardHeaderClass => IsLongWait ? "bg-warning text-dark" : Status switch
        {
            TableStatus.Free     => "bg-success text-white",
            TableStatus.Occupied => "bg-danger text-white",
            TableStatus.Reserved => "bg-warning text-dark",
            TableStatus.Cleaning => "bg-info text-dark",
            _                    => "bg-secondary text-white"
        };
    }
}
