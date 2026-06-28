using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IOrderService    _orderService;
        private const string SessionOrderId = "CurrentOrderId";

        public OrderController(IMenuItemService menuItemService, IOrderService orderService)
        {
            _menuItemService = menuItemService;
            _orderService    = orderService;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult StartOrder(int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            try
            {
                SetOrderSession(_orderService.StartOrder(tableId, EmployeeId!.Value));
                return RedirectToAction(nameof(TakeOrder), new { tableId });
            }
            catch (InvalidOperationException ex)
            {
                SetError(ex.Message);
                return RedirectToAction(nameof(Menu), new { tableId });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult LoadOrder(int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            var order = _orderService.GetOrderByTableId(tableId);
            if (order == null)
            {
                // No open order — previous orders were already sent; start a new round
                try
                {
                    SetOrderSession(_orderService.StartOrder(tableId, EmployeeId!.Value));
                }
                catch (InvalidOperationException ex)
                {
                    SetError(ex.Message);
                    return RedirectToAction(nameof(Menu), new { tableId });
                }
            }
            else
            {
                SetOrderSession(order.OrderId);
            }
            return RedirectToAction(nameof(TakeOrder), new { tableId });
        }

        [HttpGet]
        public IActionResult Menu(int tableId, MenuItemType? type, CourseType? course, CardType? card)
        {
            if (WaiterGuard() is { } r) return r;
            return View(CreateMenuViewModel(tableId, type, course, card));
        }

        [HttpGet]
        public IActionResult TakeOrder(int tableId, MenuItemType? type, CourseType? course, CardType? card)
        {
            if (WaiterGuard() is { } r) return r;
            int orderId = GetOrderSession();
            if (orderId == 0) return RedirectToAction(nameof(Menu), new { tableId, type, course, card });
            return View(CreateTakeOrderViewModel(orderId, tableId, type, course, card));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AddItem(int menuItemId, int tableId, MenuItemType? type, CourseType? course, CardType? card)
        {
            if (WaiterGuard() is { } r) return r;
            int orderId = GetOrderSession();
            if (orderId == 0) { SetError("Please start an order before adding items."); return RedirectToAction(nameof(Menu), new { tableId }); }
            try
            {
                string name = _orderService.GetItemNameById(menuItemId);
                _orderService.AddItemToOrder(orderId, menuItemId);
                SetSuccess($"'{name}' added to order.");
            }
            catch (InvalidOperationException ex) { SetError(ex.Message); }
            return RedirectToTakeOrder(tableId, type, course, card);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SendOrder(int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            int orderId = GetOrderSession();
            if (orderId == 0) { SetError("No active order found."); return RedirectToAction(nameof(Menu), new { tableId }); }
            try
            {
                EnsureOrderNotEmpty(orderId);
                _orderService.SaveOrder(orderId);
                ClearOrderSession();
                SetSuccess("Order sent successfully to the kitchen.");
                return OverviewRedirect();
            }
            catch (InvalidOperationException ex) { SetError(ex.Message); return RedirectToAction(nameof(TakeOrder), new { tableId }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult IncreaseItem(int orderItemId, int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            try
            {
                string name = _orderService.GetItemNameByOrderItemId(orderItemId);
                _orderService.IncreaseItemQuantity(orderItemId);
                // Fetch real quantity from service to display accurate message
                var item = _orderService.GetOrderItemById(orderItemId);
                SetSuccess($"'{name}' quantity increased to {item?.Quantity}.");
            }
            catch (InvalidOperationException ex) { SetError(ex.Message); }
            return RedirectToAction(nameof(TakeOrder), new { tableId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult DecreaseItem(int orderItemId, int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            try
            {
                string name = _orderService.GetItemNameByOrderItemId(orderItemId);
                _orderService.DecreaseItemQuantity(orderItemId);
                // Fetch real quantity from service to display accurate message
                var item = _orderService.GetOrderItemById(orderItemId);
                if (item?.Quantity > 1)
                    SetSuccess($"'{name}' quantity decreased to {item.Quantity}.");
            }
            catch (InvalidOperationException ex) { SetError(ex.Message); }
            return RedirectToAction(nameof(TakeOrder), new { tableId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateComment(int orderItemId, string? comment, int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            try { _orderService.UpdateItemComment(orderItemId, comment); SetSuccess("Comment updated."); }
            catch (InvalidOperationException ex) { SetError(ex.Message); }
            return RedirectToAction(nameof(TakeOrder), new { tableId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int orderItemId, int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            try
            {
                string name = _orderService.GetItemNameByOrderItemId(orderItemId);
                _orderService.RemoveItem(orderItemId);
                SetSuccess($"'{name}' removed from order.");
            }
            catch (InvalidOperationException ex) { SetError(ex.Message); }
            return RedirectToAction(nameof(TakeOrder), new { tableId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult MarkServed(int orderItemId, int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            try
            {
                string name = _orderService.GetItemNameByOrderItemId(orderItemId);
                _orderService.MarkItemServed(orderItemId);
                SetSuccess($"'{name}' marked as served.");
            }
            catch (InvalidOperationException ex) { SetError(ex.Message); }
            return RedirectToAction(nameof(TakeOrder), new { tableId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int orderId, int tableId)
        {
            if (WaiterGuard() is { } r) return r;
            try
            {
                _orderService.CancelOrder(orderId, tableId);
                ClearOrderSession();
                SetSuccess("Order cancelled successfully.");
                return OverviewRedirect();
            }
            catch (InvalidOperationException ex) { SetError(ex.Message); return RedirectToAction(nameof(TakeOrder), new { tableId }); }
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private TakeOrderViewModel CreateTakeOrderViewModel(int orderId, int tableId, MenuItemType? type, CourseType? course, CardType? card) => new()
        {
            OrderId             = orderId,
            CurrentItems        = _orderService.GetItemsByOrderId(orderId),
            SentItems           = _orderService.GetSentItemsByTableId(tableId),
            OrderStatus         = _orderService.GetOrderById(orderId)?.Status ?? Models.Enums.OrderStatus.Open,
            Menu                = CreateMenuViewModel(tableId, type, course, card),
            ConfirmationMessage = TempData["ConfirmMessage"] as string,
            ErrorMessage        = TempData["ErrorMessage"]   as string
        };

        private MenuViewModel CreateMenuViewModel(int tableId, MenuItemType? type, CourseType? course, CardType? card) => new()
        {
            MenuItems    = _menuItemService.GetFilteredMenuItems(type, course, card),
            ActiveType   = type,
            ActiveCourse = course,
            ActiveCard   = card,
            TableId      = tableId
        };

        private void EnsureOrderNotEmpty(int orderId)
        {
            if (!_orderService.GetItemsByOrderId(orderId).Any())
                throw new InvalidOperationException("Order cannot be empty. Add items before sending.");
        }

        private IActionResult RedirectToTakeOrder(int tableId, MenuItemType? type, CourseType? course, CardType? card) =>
            RedirectToAction(nameof(TakeOrder), new { tableId, type = (int?)type, course = (int?)course, card = (int?)card });

        private int  GetOrderSession()            => HttpContext.Session.GetInt32(SessionOrderId) ?? 0;
        private void SetOrderSession(int orderId) => HttpContext.Session.SetInt32(SessionOrderId, orderId);
        private void ClearOrderSession()          => HttpContext.Session.Remove(SessionOrderId);
    }
}
