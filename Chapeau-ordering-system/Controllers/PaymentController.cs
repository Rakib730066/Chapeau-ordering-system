using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService) => _paymentService = paymentService;

        [HttpGet]
        public IActionResult ViewOrder(int tableId)
        {
            if (AuthGuard() is { } r) return r;
            var model = _paymentService.GetOrderForPayment(tableId);
            if (model == null) { SetError($"No open order found for table {tableId}."); return OverviewRedirect(); }
            return View(model);
        }

        [HttpGet]
        public IActionResult FinishOrder(int tableId)
        {
            if (AuthGuard() is { } r) return r;
            var model = _paymentService.GetFinishOrderViewModel(tableId);
            if (model == null) { SetError($"No open order found for table {tableId}."); return OverviewRedirect(); }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult FinishOrder(FinishOrderViewModel input)
        {
            if (AuthGuard() is { } r) return r;
            if (!ModelState.IsValid) return View(RestoreFinishOrderForm(input));
            return ProcessFinishOrder(input);
        }

        [HttpGet]
        public IActionResult SplitOrder(int tableId)
        {
            if (AuthGuard() is { } r) return r;
            var model = _paymentService.GetSplitPaymentViewModel(tableId);
            if (model == null) { SetError($"No open order found for table {tableId}."); return OverviewRedirect(); }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SplitOrder(SplitPaymentViewModel input)
        {
            if (AuthGuard() is { } r) return r;
            var vm = _paymentService.GetSplitPaymentViewModel(input.TableId);
            if (vm == null) { SetError($"No open order found for table {input.TableId}."); return OverviewRedirect(); }
            ApplySplitPaymentInput(vm, input);
            if (SplitNeedsRecalculation(vm)) return View(RecalculateEqualSplit(vm));
            if (!ModelState.IsValid) return View(vm);
            return CompleteSplitPayment(vm, input.TableNumber);
        }

        [HttpGet]
        public IActionResult Confirmation(string tableNumber)
        {
            if (AuthGuard() is { } r) return r;
            ViewData["TableNumber"] = tableNumber;
            ViewData["Message"]     = TempData["ConfirmationMessage"];
            return View();
        }

        private FinishOrderViewModel RestoreFinishOrderForm(FinishOrderViewModel input)
        {
            var vm = _paymentService.GetFinishOrderViewModel(input.TableId) ?? input;
            vm.AmountPaid    = input.AmountPaid;
            vm.PaymentMethod = input.PaymentMethod;
            vm.Feedback      = input.Feedback;
            return vm;
        }

        private IActionResult ProcessFinishOrder(FinishOrderViewModel input)
        {
            try
            {
                _paymentService.FinishOrder(input);
                TempData["ConfirmationMessage"] = $"Order for table {input.TableNumber} finished successfully.";
                return RedirectToAction(nameof(Confirmation), new { tableNumber = input.TableNumber });
            }
            catch (Exception ex)
            {
                SetError($"Could not finish the order: {ex.Message}");
                return OverviewRedirect();
            }
        }

        private static void ApplySplitPaymentInput(SplitPaymentViewModel vm, SplitPaymentViewModel input)
        {
            vm.Mode           = input.Mode;
            vm.NumberOfPeople = input.NumberOfPeople;
            vm.Payments       = input.Payments ?? new List<PersonPaymentViewModel>();
        }

        private bool SplitNeedsRecalculation(SplitPaymentViewModel vm)
        {
            bool isRecalc = Request.Form["action"] == "recalc";
            bool mismatch = vm.Mode == SplitMode.Equal && vm.Payments.Count != vm.NumberOfPeople;
            return isRecalc || mismatch;
        }

        private SplitPaymentViewModel RecalculateEqualSplit(SplitPaymentViewModel vm)
        {
            ModelState.Clear();
            return _paymentService.RebuildEqualSplit(vm);
        }

        private IActionResult CompleteSplitPayment(SplitPaymentViewModel vm, string tableNumber)
        {
            var (success, error) = _paymentService.FinishSplitOrder(vm);
            if (!success) { ModelState.AddModelError("", error ?? "Could not finish split payment."); return View(vm); }
            TempData["ConfirmationMessage"] = $"Order for table {tableNumber} finished ({vm.Payments.Count} payments).";
            return RedirectToAction(nameof(Confirmation), new { tableNumber });
        }
    }
}
