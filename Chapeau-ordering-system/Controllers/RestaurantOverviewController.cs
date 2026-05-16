using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class RestaurantOverviewController : Controller
    {
        private readonly ITableService _tableService;

        public RestaurantOverviewController(ITableService tableService)
        {
            _tableService = tableService;
        }

        public IActionResult Index()
        {
            string? employeeRole = HttpContext.Session.GetString("EmployeeRole");
            if (string.IsNullOrEmpty(employeeRole))
                return RedirectToAction("Login", "Account");

            ViewData["EmployeeName"] = HttpContext.Session.GetString("EmployeeName");
            ViewData["EmployeeRole"] = employeeRole;

            var tables = _tableService.GetAllTables().ToList();
            return View(tables);
        }
    }
}