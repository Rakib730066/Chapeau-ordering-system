using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public IActionResult ViewOrder(int tableId)
        {
            if (HttpContext.Session.GetInt32("EmployeeId") == null)
                return RedirectToAction("Login", "Account");

            var model = _paymentService.GetOrderForPayment(tableId);
            if (model == null)
            {
                TempData["Message"] = $"No open order found for table {tableId}.";
                return RedirectToAction("Index", "RestaurantOverview");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult FinishOrder(int tableId)
        {
            if (HttpContext.Session.GetInt32("EmployeeId") == null)
                return RedirectToAction("Login", "Account");

            var model = _paymentService.GetFinishOrderViewModel(tableId);
            if (model == null)
            {
                TempData["Message"] = $"No open order found for table {tableId}.";
                return RedirectToAction("Index", "RestaurantOverview");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinishOrder(FinishOrderViewModel input)
        {
            if (HttpContext.Session.GetInt32("EmployeeId") == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                // Re-fetch totals so the form re-renders with the bill summary
                var refreshed = _paymentService.GetFinishOrderViewModel(input.TableId);
                if (refreshed != null)
                {
                    // keep the waiter's inputs but restore the calculated values
                    refreshed.AmountPaid = input.AmountPaid;
                    refreshed.PaymentMethod = input.PaymentMethod;
                    refreshed.Feedback = input.Feedback;
                    return View(refreshed);
                }
                return View(input);
            }

            try
            {
                _paymentService.FinishOrder(input);
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Could not finish the order: {ex.Message}";
                return RedirectToAction("ViewOrder", new { tableId = input.TableId });
            }

            TempData["ConfirmationMessage"] =
                $"Order for table {input.TableNumber} finished successfully.";
            return RedirectToAction("Confirmation", new { tableNumber = input.TableNumber });
        }

        [HttpGet]
        public IActionResult Confirmation(string tableNumber)
        {
            if (HttpContext.Session.GetInt32("EmployeeId") == null)
                return RedirectToAction("Login", "Account");

            ViewData["TableNumber"] = tableNumber;
            ViewData["Message"] = TempData["ConfirmationMessage"];
            return View();
        }
    }
}