using System.Diagnostics;
using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITableService _tableService;
        private readonly IOrderService _orderService;

        public HomeController(ILogger<HomeController> logger, ITableService tableService, IOrderService orderService)
        {
            _logger = logger;
            _tableService = tableService;
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            string? employeeRole = HttpContext.Session.GetString("EmployeeRole");
            ViewData["EmployeeRole"] = employeeRole;

            var model = new RestaurantOverviewViewModel
            {
                Tables = _tableService.GetAllTables(),
                OpenOrders = _orderService.GetOpenOrders()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
