using Microsoft.AspNetCore.Mvc;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Controllers
{
    public class BarKitchenController : Controller
    {
        private readonly IBarKitchenService _barKitchenService;

        public BarKitchenController(IBarKitchenService barKitchenService)
        {
            _barKitchenService = barKitchenService;
        }

        // Only Bar and Kitchen employees can access this page
        private bool IsBarOrKitchenEmployee()
        {
            string? role = HttpContext.Session.GetString("EmployeeRole");
            return role == "Bar" || role == "Kitchen";
        }

        public IActionResult Index(string viewMode = "running")
        {
            if (!IsBarOrKitchenEmployee())
                return RedirectToAction("Login", "Account");
            
            BarKitchenViewModel viewModel;
            if (viewMode == "finished")
            {
                viewModel = _barKitchenService.GetFinishedOrdersTodayViewModel();
            }
            else
            {
                viewModel = _barKitchenService.GetRunningOrdersViewModel();
            }

            viewModel.ErrorMessage = TempData["ErrorMessage"] as string;
            viewModel.ConfirmMessage = TempData["ConfirmMessage"] as string;

            return View(viewModel);
        }

        private void SetStatusMessage(bool isSuccessful, string confirmMessage, string errorMessage)
        {
            if (isSuccessful)
                TempData["ConfirmMessage"] = confirmMessage;
            else
                TempData["ErrorMessage"] = errorMessage;
        }

        // POST: Mark one order item as being prepared
        [HttpPost]
        public IActionResult StartItem(int orderItemId)
        {
            if (!IsBarOrKitchenEmployee())
                return RedirectToAction("Login", "Account");

            bool isSuccessful = _barKitchenService.MarkItemBeingPrepared(orderItemId);
            SetStatusMessage(
                isSuccessful,
                "Order item started successfully.",
                "Could not start the order item. It may already be in progress, ready, or unavailable.");

            return RedirectToAction("Index", new { viewMode = "running" });
        }

        // POST: Mark order item as ready to be served
        [HttpPost]
        public IActionResult MarkItemReady(int orderItemId)
        {
            if (!IsBarOrKitchenEmployee())
                return RedirectToAction("Login", "Account");

            bool isSuccessful = _barKitchenService.MarkItemReady(orderItemId);
            SetStatusMessage(
                isSuccessful,
                "Order item marked as ready.",
                "Could not mark the order item as ready. It may not be in preparation or may be unavailable.");

            return RedirectToAction("Index", new { viewMode = "running" });
        }
        // POST: Mark ALL items in order as being prepared
        [HttpPost]
        public IActionResult StartOrder(int orderId)
        {
            if (!IsBarOrKitchenEmployee())
                return RedirectToAction("Login", "Account");

            bool isSuccessful = _barKitchenService.MarkOrderBeingPrepared(orderId);
            SetStatusMessage(
                isSuccessful,
                "Order started successfully.",
                "Could not start the order. There may be no ordered items to start.");

            return RedirectToAction("Index", new { viewMode = "running" });
        }

        // POST: Mark ALL items in order as ready to be served
        [HttpPost]
        public IActionResult MarkOrderReady(int orderId)
        {
            if (!IsBarOrKitchenEmployee())
                return RedirectToAction("Login", "Account");

            bool isSuccessful = _barKitchenService.MarkOrderReadyToServe(orderId);
            SetStatusMessage(
                isSuccessful,
                "Order marked as ready.",
                "Could not mark the order as ready. There may be no items that can be updated.");

            return RedirectToAction("Index", new { viewMode = "finished" });
        }

        // POST: Mark all items in a course as being prepared (Kitchen only)
        [HttpPost]
        public IActionResult StartCourse(int orderId, int courseType)
        {
            if (!IsBarOrKitchenEmployee())
                return RedirectToAction("Login", "Account");

            bool isSuccessful = _barKitchenService.MarkCourseBeingPrepared(orderId, courseType);
            SetStatusMessage(
                isSuccessful,
                "Course started successfully.",
                "Could not start the course. It may be invalid, already started, or unavailable.");

            return RedirectToAction("Index", new { viewMode = "running" });
        }

        // POST: Mark all items in a course as ready to be served (Kitchen only)
        [HttpPost]
        public IActionResult MarkCourseReady(int orderId, int courseType)
        {
            if (!IsBarOrKitchenEmployee())
                return RedirectToAction("Login", "Account");

            bool isSuccessful = _barKitchenService.MarkCourseReadyToServe(orderId, courseType);
            SetStatusMessage(
                isSuccessful,
                "Course marked as ready.",
                "Could not mark the course as ready. It may not be in preparation or may be unavailable.");

            return RedirectToAction("Index", new { viewMode = "running" });
        }
    }
}
