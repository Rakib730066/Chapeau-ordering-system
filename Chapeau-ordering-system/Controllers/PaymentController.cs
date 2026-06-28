using Chapeau_ordering_system.Models.Enums;
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
                TempData["ErrorMessage"] = $"No open order found for table {tableId}.";
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
                TempData["ErrorMessage"] = $"No open order found for table {tableId}.";
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
                var refreshedForm = _paymentService.GetFinishOrderViewModel(input.TableId);
                if (refreshedForm != null)
                {
                    refreshedForm.AmountPaid = input.AmountPaid;
                    refreshedForm.PaymentMethod = input.PaymentMethod;
                    refreshedForm.Feedback = input.Feedback;
                    return View(refreshedForm);
                }
                return View(input);
            }

            try
            {
                _paymentService.FinishOrder(input);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not finish the order: {ex.Message}";
                return RedirectToAction("Index", "RestaurantOverview");
            }

            TempData["ConfirmationMessage"] =
                $"Order for table {input.TableNumber} finished successfully.";
            return RedirectToAction("Confirmation", new { tableNumber = input.TableNumber });
        }

        [HttpGet]
        public IActionResult SplitOrder(int tableId)
        {
            if (HttpContext.Session.GetInt32("EmployeeId") == null)
                return RedirectToAction("Login", "Account");

            var model = _paymentService.GetSplitPaymentViewModel(tableId);
            if (model == null)
            {
                TempData["ErrorMessage"] = $"No open order found for table {tableId}.";
                return RedirectToAction("Index", "RestaurantOverview");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SplitOrder(SplitPaymentViewModel input)
        {
            if (HttpContext.Session.GetInt32("EmployeeId") == null)
                return RedirectToAction("Login", "Account");

            var refreshed = _paymentService.GetSplitPaymentViewModel(input.TableId);
            if (refreshed == null)
            {
                TempData["ErrorMessage"] = $"No open order found for table {input.TableId}.";
                return RedirectToAction("Index", "RestaurantOverview");
            }

            refreshed.Mode = input.Mode;
            refreshed.NumberOfPeople = input.NumberOfPeople;
            refreshed.Payments = input.Payments ?? new List<PersonPaymentViewModel>();

            // Rebuild rows when the people count changes.
            bool isRecalc = Request.Form["action"] == "recalc";
            bool peopleCountMismatch = refreshed.Payments.Count != refreshed.NumberOfPeople;

            if (isRecalc || peopleCountMismatch)
            {
                refreshed = _paymentService.RebuildEqualSplit(refreshed);
                ModelState.Clear();
                return View(refreshed);
            }

            if (!ModelState.IsValid)
                return View(refreshed);

            var (success, errorMessage) = _paymentService.FinishSplitOrder(refreshed);

            if (!success)
            {
                ModelState.AddModelError("", errorMessage ?? "Could not finish split payment.");
                return View(refreshed);
            }

            TempData["ConfirmationMessage"] =
                $"Order for table {input.TableNumber} finished successfully ({refreshed.Payments.Count} payments).";
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