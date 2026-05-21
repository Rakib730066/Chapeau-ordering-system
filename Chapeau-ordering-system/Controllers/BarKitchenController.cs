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

        // Simple session check
        private bool IsEmployeeLoggedIn()
        {
            return HttpContext.Session.GetString("EmployeeRole") != null;
        }

        public IActionResult Index()
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");
            var viewModel = _barKitchenService.GetBarKitchenViewModel();
            return View(viewModel);
        }

        
        [HttpPost]
        public IActionResult StartItem(int orderItemId)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");
            _barKitchenService.MarkItemBeingPrepared(orderItemId);
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        public IActionResult MarkItemReady(int orderItemId)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");

            _barKitchenService.MarkItemReady(orderItemId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult StartOrder(int orderId)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");
            _barKitchenService.MarkOrderBeingPrepared(orderId);
            return RedirectToAction("Index");
        }

        // POST: Mark ALL items in order as ready to be served
        [HttpPost]
        public IActionResult MarkOrderReady(int orderId)
        {
            if (!IsEmployeeLoggedIn())
                return RedirectToAction("Login", "Account");

            _barKitchenService.MarkOrderReadyToServe(orderId);
            return RedirectToAction("Index");
        }
    }
}
