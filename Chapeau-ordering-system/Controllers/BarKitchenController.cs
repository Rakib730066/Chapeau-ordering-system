using Chapeau_ordering_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class BarKitchenController : BaseController
    {
        private readonly IBarKitchenService _barKitchenService;
        public BarKitchenController(IBarKitchenService barKitchenService) => _barKitchenService = barKitchenService;

        public IActionResult Index(string viewMode = "running")
        {
            if (BarKitchenGuard() is { } r) return r;
            var vm = viewMode == "finished"
                ? _barKitchenService.GetFinishedOrdersTodayViewModel()
                : _barKitchenService.GetRunningOrdersViewModel();
            vm.ErrorMessage   = TempData["ErrorMessage"]   as string;
            vm.ConfirmMessage = TempData["ConfirmMessage"] as string;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult StartItem(int orderItemId) =>
            ExecuteBarKitchenAction(() => _barKitchenService.MarkItemBeingPrepared(orderItemId),
                "Order item started.", "Could not start the order item.");

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult MarkItemReady(int orderItemId) =>
            ExecuteBarKitchenAction(() => _barKitchenService.MarkItemReady(orderItemId),
                "Order item marked as ready.", "Could not mark the order item as ready.");

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult StartOrder(int orderId) =>
            ExecuteBarKitchenAction(() => _barKitchenService.MarkOrderBeingPrepared(orderId),
                "Order started.", "Could not start the order.");

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult MarkOrderReady(int orderId) =>
            ExecuteBarKitchenAction(() => _barKitchenService.MarkOrderReadyToServe(orderId),
                "Order marked as ready.", "Could not mark the order as ready.", "finished");

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult StartCourse(int orderId, int courseType) =>
            ExecuteBarKitchenAction(() => _barKitchenService.MarkCourseBeingPrepared(orderId, courseType),
                "Course started.", "Could not start the course.");

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult MarkCourseReady(int orderId, int courseType) =>
            ExecuteBarKitchenAction(() => _barKitchenService.MarkCourseReadyToServe(orderId, courseType),
                "Course marked as ready.", "Could not mark the course as ready.");

        private IActionResult ExecuteBarKitchenAction(Func<bool> action, string success, string failure, string viewMode = "running")
        {
            if (BarKitchenGuard() is { } r) return r;
            if (action()) SetSuccess(success); else SetError(failure);
            return RedirectToAction(nameof(Index), new { viewMode });
        }
    }
}
