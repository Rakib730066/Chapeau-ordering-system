using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class RestaurantOverviewController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IOrderService _orderService;

        public RestaurantOverviewController(ITableService tableService, IOrderService orderService)
        {
            _tableService = tableService;
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            string? employeeRole = HttpContext.Session.GetString("EmployeeRole");
            if (string.IsNullOrEmpty(employeeRole))
                return RedirectToAction("Login", "Account");

            ViewData["EmployeeName"] = HttpContext.Session.GetString("EmployeeName");
            ViewData["EmployeeRole"] = employeeRole;

            var model = new RestaurantOverviewViewModel
            {
                Tables     = _tableService.GetAllTables(),
                OpenOrders = _orderService.GetOpenOrders()
            };
            return View(model);
        }
    }
}
