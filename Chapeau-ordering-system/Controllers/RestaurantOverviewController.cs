using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class RestaurantOverviewController : Controller
    {
        public IActionResult Index()
        {
            string? employeeRole = HttpContext.Session.GetString("EmployeeRole");
            if (string.IsNullOrEmpty(employeeRole))
                return RedirectToAction("Login", "Account");

            ViewData["EmployeeName"] = HttpContext.Session.GetString("EmployeeName");
            ViewData["EmployeeRole"] = employeeRole;

            return View();
        }
    }
}