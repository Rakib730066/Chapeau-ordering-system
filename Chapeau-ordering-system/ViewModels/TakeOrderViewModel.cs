using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.ViewModels
{
    public class TakeOrderViewModel
    {
        public MenuViewModel Menu { get; set; } = new MenuViewModel();

        public List<OrderItem> CurrentItems { get; set; } = new List<OrderItem>();

        public int OrderId { get; set; }

        public decimal TotalPrice => CurrentItems.Sum(i => i.MenuItem!.Price * i.Quantity);

        public string? ConfirmationMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}