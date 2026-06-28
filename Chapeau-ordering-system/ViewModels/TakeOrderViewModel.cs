using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.ViewModels
{
    public class TakeOrderViewModel
    {
        public MenuViewModel Menu { get; set; } = new MenuViewModel();

        public List<OrderItem> CurrentItems { get; set; } = new List<OrderItem>();

        public List<OrderItem> SentItems { get; set; } = new List<OrderItem>();

        public int OrderId { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public bool IsSubmitted => OrderStatus == OrderStatus.Submitted;

        public decimal TotalPrice      => CurrentItems.Sum(i => i.MenuItem!.Price * i.Quantity);
        public int     CurrentItemCount => CurrentItems.Sum(i => i.Quantity);
        public int     SentItemCount    => SentItems.Sum(i => i.Quantity);
        public decimal SentItemTotal    => SentItems.Sum(i => i.MenuItem!.Price * i.Quantity);
        public decimal TableTotal       => SentItemTotal + TotalPrice;

        public string? ConfirmationMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}