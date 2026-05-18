using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IBarKitchenService
    {
        // Get Kitchen ViewModel with food orders
        BarKitchenViewModel GetKitchenViewModel();

        // Get Bar ViewModel with drink orders
        BarKitchenViewModel GetBarViewModel();

        // Mark order item as being prepared
        void MarkItemBeingPrepared(int orderItemId);

        // Mark order item as ready to be served
        void MarkItemReady(int orderItemId);
    }
}
