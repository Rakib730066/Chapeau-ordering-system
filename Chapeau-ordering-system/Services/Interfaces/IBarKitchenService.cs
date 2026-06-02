using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IBarKitchenService
    {
        // Get ViewModel based on logged-in employee role (Kitchen or Bar)
        BarKitchenViewModel GetBarKitchenViewModel();

        // Get running orders view
        BarKitchenViewModel GetRunningOrdersViewModel();

        // Get finished orders today view
        BarKitchenViewModel GetFinishedOrdersTodayViewModel();

        // Mark order item as being prepared (service determines role internally)
        void MarkItemBeingPrepared(int orderItemId);

        // Mark order item as ready to be served (service determines role internally)
        void MarkItemReady(int orderItemId);

        // Mark all items in an order as being prepared (service determines role internally)
        void MarkOrderBeingPrepared(int orderId);

        // Mark all items in an order as ready to be served (service determines role internally)
        void MarkOrderReadyToServe(int orderId);

        // Mark all items in a course as being prepared (Kitchen only)
        void MarkCourseBeingPrepared(int orderId, CourseType courseType);

        // Mark all items in a course as ready to be served (Kitchen only)
        void MarkCourseReadyToServe(int orderId, CourseType courseType);
    }
}
