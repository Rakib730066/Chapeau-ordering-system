using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.ViewModels
{
    public class BarKitchenViewModel
    {
        // Page title to display (e.g., "Kitchen Orders" or "Bar Orders")
        public string PageTitle { get; set; } = string.Empty;

        // Return page for redirect after POST actions (e.g., "Kitchen" or "Bar")
        public string ReturnPage { get; set; } = string.Empty;

        // Menu item type (Food for Kitchen, Drink for Bar)
        public MenuItemType MenuItemType { get; set; }

        // List of running orders to display
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}

