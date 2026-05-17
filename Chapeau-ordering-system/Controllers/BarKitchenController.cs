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

        // Single view for Bar and Kitchen - determines role from logged-in employee
        public IActionResult Index()
        {
            // Get the bar/kitchen view
            BarKitchenViewModel viewModel = _barKitchenService.GetKitchenViewModel();
            return View(viewModel);
        }

        // Start preparing an item
        [HttpPost]
        public IActionResult StartItem(int orderItemId)
        {
            _barKitchenService.MarkItemBeingPrepared(orderItemId);
            TempData["ConfirmMessage"] = "Item preparation started.";
            return RedirectToAction("Index");
        }

        // Mark item as ready to be served
        [HttpPost]
        public IActionResult MarkItemReady(int orderItemId)
        {
            _barKitchenService.MarkItemReady(orderItemId);
            TempData["ConfirmMessage"] = "Item marked as ready.";
            return RedirectToAction("Index");
        }
    }
}
