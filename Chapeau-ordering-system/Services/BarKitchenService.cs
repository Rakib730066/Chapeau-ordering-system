using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Services
{
    public class BarKitchenService : IBarKitchenService
    {
        private readonly IBarKitchenRepository _orderRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        

        public BarKitchenService(IBarKitchenRepository orderRepository, IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper: Get MenuItemType based on logged-in employee role
        private MenuItemType GetMenuItemTypeForCurrentEmployee()
        {
            var roleString = _httpContextAccessor.HttpContext?.Session.GetString("EmployeeRole");
            
            if (roleString == null)
                throw new InvalidOperationException("Employee role not found in session.");

            if (Enum.TryParse<EmployeeRole>(roleString, out var role))
            {
                return role == EmployeeRole.Bar ? MenuItemType.Drink : MenuItemType.Food;
            }

            throw new InvalidOperationException("Invalid employee role in session.");
        }

        // Get ViewModel based on logged-in employee role
        public BarKitchenViewModel GetBarKitchenViewModel()
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            BarKitchenViewModel viewModel = new BarKitchenViewModel();
            viewModel.MenuItemType = menuItemType;
            viewModel.PageTitle = menuItemType == MenuItemType.Food ? "Kitchen Orders" : "Bar Orders";
            viewModel.ReturnPage = menuItemType == MenuItemType.Food ? "Kitchen" : "Bar";
            viewModel.ViewMode = "running";
            viewModel.IsFinishedView = false;
            viewModel.Orders = _orderRepository.GetRunningOrders(menuItemType);

            return viewModel;
        }

        // Get running orders view
        public BarKitchenViewModel GetRunningOrdersViewModel()
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            BarKitchenViewModel viewModel = new BarKitchenViewModel();
            viewModel.MenuItemType = menuItemType;
            viewModel.PageTitle = menuItemType == MenuItemType.Food ? "Kitchen Orders" : "Bar Orders";
            viewModel.ReturnPage = menuItemType == MenuItemType.Food ? "Kitchen" : "Bar";
            viewModel.ViewMode = "running";
            viewModel.IsFinishedView = false;
            viewModel.Orders = _orderRepository.GetRunningOrders(menuItemType);

            return viewModel;
        }

        // Get finished orders today view
        public BarKitchenViewModel GetFinishedOrdersTodayViewModel()
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            BarKitchenViewModel viewModel = new BarKitchenViewModel();
            viewModel.MenuItemType = menuItemType;
            viewModel.PageTitle = menuItemType == MenuItemType.Food ? "Kitchen Orders - Finished Today" : "Bar Orders - Finished Today";
            viewModel.ReturnPage = menuItemType == MenuItemType.Food ? "Kitchen" : "Bar";
            viewModel.ViewMode = "finished";
            viewModel.IsFinishedView = true;
            viewModel.Orders = _orderRepository.GetFinishedOrdersToday(menuItemType);

            return viewModel;
        }

        // Mark order item as being prepared
        public void MarkItemBeingPrepared(int orderItemId)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            _orderRepository.UpdateOrderItemStatus(
                orderItemId,
                menuItemType,
                OrderItemStatus.Ordered,
                OrderItemStatus.BeingPrepared);
        }

        // Mark order item as ready to be served
        public void MarkItemReady(int orderItemId)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            _orderRepository.UpdateOrderItemStatus(
                orderItemId,
                menuItemType,
                OrderItemStatus.BeingPrepared,
                OrderItemStatus.ReadyToBeServed);
        }

        // Mark all items in an order as being prepared
        public void MarkOrderBeingPrepared(int orderId)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            _orderRepository.UpdateOrderItemsStatusForOrder(
                orderId,
                menuItemType,
                OrderItemStatus.Ordered,
                OrderItemStatus.BeingPrepared);
        }

        // Mark all items in an order as ready to be served
        public void MarkOrderReadyToServe(int orderId)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            _orderRepository.UpdateAllOrderItemsToReady(orderId, menuItemType);
        }

        // Mark all items in a course as being prepared (Kitchen only)
        public void MarkCourseBeingPrepared(int orderId, CourseType courseType)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();
            
            // Only allow Kitchen to update courses
            if (menuItemType == MenuItemType.Food)
            {
                _orderRepository.UpdateCourseStatus(
                    orderId,
                    courseType,
                    OrderItemStatus.Ordered,
                    OrderItemStatus.BeingPrepared);
            }
        }

        // Mark all items in a course as ready to be served (Kitchen only)
        public void MarkCourseReadyToServe(int orderId, CourseType courseType)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();
            
            // Only allow Kitchen to update courses
            if (menuItemType == MenuItemType.Food)
            {
                _orderRepository.UpdateCourseStatus(
                    orderId,
                    courseType,
                    OrderItemStatus.BeingPrepared,
                    OrderItemStatus.ReadyToBeServed);
            }
        }
    }
}
