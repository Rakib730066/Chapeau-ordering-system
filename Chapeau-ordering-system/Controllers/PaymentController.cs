using Chapeau_ordering_system.Services.Interfaces;
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
    }
}