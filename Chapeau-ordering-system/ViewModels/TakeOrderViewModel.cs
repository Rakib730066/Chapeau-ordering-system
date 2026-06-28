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

        // VAT breakdown: group all items (sent + current) by VAT rate
        public IEnumerable<(decimal Rate, decimal Amount)> VatBreakdown =>
            SentItems.Concat(CurrentItems)
                     .Where(i => i.MenuItem != null)
                     .GroupBy(i => i.MenuItem!.VatRate)
                     .Where(g => g.Key > 0)
                     .OrderBy(g => g.Key)
                     .Select(g => (
                         Rate:   g.Key,
                         Amount: g.Sum(i => i.MenuItem!.Price * i.Quantity * (g.Key / (100m + g.Key)))
                     ));

        public string OrderPanelTitle  => SentItems.Any() ? "New Order" : "Current Order";
        public string? WaiterName      { get; set; }
        public string? ConfirmationMessage { get; set; }
        public string? ErrorMessage        { get; set; }
    }
}