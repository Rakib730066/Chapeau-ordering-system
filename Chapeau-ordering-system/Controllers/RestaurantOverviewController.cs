using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class RestaurantOverviewController : BaseController
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
            if (AuthGuard() is { } r) return r;
            return View(new RestaurantOverviewViewModel
            {
                Tables       = _tableService.GetAllTables()
                                   .OrderBy(t => int.TryParse(t.TableNumber.Replace("T", ""), out int n) ? n : 0)
                                   .ToList(),
                OpenOrders   = _orderService.GetOpenOrders(),
                EmployeeName = HttpContext.Session.GetString("EmployeeName"),
                EmployeeRole = Role
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int tableId, int newStatus)
        {
            if (AuthGuard() is { } r) return r;
            var status = (TableStatus)newStatus;
            if (status == TableStatus.Free && _orderService.TableHasUnservedItems(tableId))
            {
                SetError("Cannot mark table as free — it still has unserved items.");
                return OverviewRedirect();
            }
            _tableService.UpdateStatus(tableId, status);
            return OverviewRedirect();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult MarkServed(int orderItemId)
        {
            if (AuthGuard() is { } r) return r;
            _orderService.MarkItemServed(orderItemId);
            return OverviewRedirect();
        }
    }
}
