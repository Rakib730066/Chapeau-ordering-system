using Microsoft.AspNetCore.Mvc;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Controllers
{
    public class BarKitchenController : Controller
    {
        private readonly IBarKitchenService _barKitchenService;

        public BarKitchenController(IBarKitchenService barKitchenService)
        {
            _barKitchenService = barKitchenService;
        }

        // Simple session check
        private bool IsEmployeeLoggedIn()
        {
            return HttpContext.Session.GetString("EmployeeRole") != null;
        }

        public IActionResult Index(string viewMode = "running")
        {
            if (!IsEmployeeLoggedIn())
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
            return View(viewModel);
        }

        
        [HttpPost]
        public IActionResult StartItem(int orderItemId)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");
            _barKitchenService.MarkItemBeingPrepared(orderItemId);
            return RedirectToAction("Index", new { viewMode = "running" });
        }

        
        [HttpPost]
        public IActionResult MarkItemReady(int orderItemId)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");

            _barKitchenService.MarkItemReady(orderItemId);
            return RedirectToAction("Index", new { viewMode = "running" });
        }

        [HttpPost]
        public IActionResult StartOrder(int orderId)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");
            _barKitchenService.MarkOrderBeingPrepared(orderId);
            return RedirectToAction("Index", new { viewMode = "running" });
        }

        // POST: Mark ALL items in order as ready to be served
        [HttpPost]
        public IActionResult MarkOrderReady(int orderId)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");

            _barKitchenService.MarkOrderReadyToServe(orderId);
            return RedirectToAction("Index", new { viewMode = "running" });
        }

        // POST: Mark all items in a course as being prepared (Kitchen only)
        [HttpPost]
        public IActionResult StartCourse(int orderId, int courseType)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");

            if (Enum.TryParse<CourseType>(courseType.ToString(), out var course))
            {
                _barKitchenService.MarkCourseBeingPrepared(orderId, course);
            }
            return RedirectToAction("Index", new { viewMode = "running" });
        }

        // POST: Mark all items in a course as ready to be served (Kitchen only)
        [HttpPost]
        public IActionResult MarkCourseReady(int orderId, int courseType)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");

            if (Enum.TryParse<CourseType>(courseType.ToString(), out var course))
            {
                _barKitchenService.MarkCourseReadyToServe(orderId, course);
            }
            return RedirectToAction("Index", new { viewMode = "running" });
        }
    }
}
