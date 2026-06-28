using Chapeau_ordering_system.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int?    EmployeeId   => HttpContext.Session.GetInt32(SessionKeys.EmployeeId);
        protected string? Role         => HttpContext.Session.GetString(SessionKeys.EmployeeRole);

        protected bool IsAuthenticated => EmployeeId.HasValue;
        protected bool IsManager       => Role == "Manager";
        protected bool IsBarOrKitchen  => Role is "Bar" or "Kitchen";
        protected bool IsWaiter        => Role == "Waiter";

        protected IActionResult  LoginRedirect()    => RedirectToAction("Login",  "Account");
        protected IActionResult  OverviewRedirect() => RedirectToAction("Index",  "RestaurantOverview");

        // Returns a redirect when the guard fails; null means the caller may continue.
        protected IActionResult? AuthGuard()       => IsAuthenticated ? null : LoginRedirect();
        protected IActionResult? ManagerGuard()    => IsManager       ? null : LoginRedirect();
        protected IActionResult? BarKitchenGuard() => IsBarOrKitchen  ? null : LoginRedirect();
        protected IActionResult? WaiterGuard()     => IsWaiter        ? null : LoginRedirect();

        protected void SetSuccess(string message) => TempData["ConfirmMessage"] = message;
        protected void SetError(string message)   => TempData["ErrorMessage"]   = message;
    }
}
