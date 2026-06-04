using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.ViewModels
{
    public class BarKitchenViewModel
    {

        public string PageTitle { get; set; } = string.Empty;

        public string ReturnPage { get; set; } = string.Empty;

        public MenuItemType MenuItemType { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();

        public string ViewMode { get; set; } = "running";

        public bool IsFinishedView { get; set; } = false;
    }
}

