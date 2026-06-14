using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IBarKitchenService
    {
        // Get running orders view
        BarKitchenViewModel GetRunningOrdersViewModel();

        // Get finished orders today view
        BarKitchenViewModel GetFinishedOrdersTodayViewModel();

        // Mark order item as being prepared (service determines role internally)
        bool MarkItemBeingPrepared(int orderItemId);

        // Mark order item as ready to be served (service determines role internally)
        bool MarkItemReady(int orderItemId);

        // Mark all items in an order as being prepared (service determines role internally)
        bool MarkOrderBeingPrepared(int orderId);

        // Mark all items in an order as ready to be served (service determines role internally)
        bool MarkOrderReadyToServe(int orderId);

        // Mark all items in a course as being prepared (Kitchen only)
        bool MarkCourseBeingPrepared(int orderId, int courseType);

        // Mark all items in a course as ready to be served (Kitchen only)
        bool MarkCourseReadyToServe(int orderId, int courseType);
    }
}
