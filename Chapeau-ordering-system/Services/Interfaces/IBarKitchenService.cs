using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IBarKitchenService
    {
        // Get ViewModel based on logged-in employee role (Kitchen or Bar)
        BarKitchenViewModel GetBarKitchenViewModel();

        // Mark order item as being prepared (service determines role internally)
        void MarkItemBeingPrepared(int orderItemId);

        // Mark order item as ready to be served (service determines role internally)
        void MarkItemReady(int orderItemId);

        // Mark all items in an order as being prepared (service determines role internally)
        void MarkOrderBeingPrepared(int orderId);

        // Mark all items in an order as ready to be served (service determines role internally)
        void MarkOrderReadyToServe(int orderId);
    }
}
