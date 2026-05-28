using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.ViewModels
{
    public class TakeOrderViewModel
    {
        // The menu items to choose from (with filters)
        public MenuViewModel Menu { get; set; } = new MenuViewModel();

        // The items currently added to this order (stored in session)
        public List<OrderItem> CurrentItems { get; set; } = new List<OrderItem>();

        // The order id once started
        public int OrderId { get; set; }

        // Confirmation message after saving
        public string? ConfirmationMessage { get; set; }

        // Total price of current items
        public decimal TotalPrice => CurrentItems.Sum(i => i.MenuItem!.Price * i.Quantity);
    }
}