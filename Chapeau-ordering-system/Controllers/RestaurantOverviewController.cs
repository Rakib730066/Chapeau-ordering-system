using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Chapeau_ordering_system.Models.Enums;


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
                Tables = _tableService.GetAllTables(),
                OpenOrders = _orderService.GetOpenOrders()
            };

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int tableId, int newStatus)
        {
            string? employeeRole = HttpContext.Session.GetString("EmployeeRole");
            if (string.IsNullOrEmpty(employeeRole))
                return RedirectToAction("Login", "Account");

            TableStatus status = (TableStatus)newStatus; // casting , converting tableStatus int to enum .

            if (status == TableStatus.Free && _orderService.TableHasUnservedItems(tableId))
            {
                TempData["Error"] = "Cannot mark table as free — it still has open orders.";
                return RedirectToAction("Index");
            }

            _tableService.UpdateStatus(tableId, status);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkServed(int orderItemId)
        {
            string? employeeRole = HttpContext.Session.GetString("EmployeeRole");
            if (string.IsNullOrEmpty(employeeRole))
                return RedirectToAction("Login", "Account");

            _orderService.MarkItemServed(orderItemId);
            return RedirectToAction("Index");
        }



    }
}
