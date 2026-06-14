using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;

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
                if (role == EmployeeRole.Bar)
                    return MenuItemType.Drink;

                if (role == EmployeeRole.Kitchen)
                    return MenuItemType.Food;
            }

            throw new InvalidOperationException("Invalid employee role in session.");
        }


        // Get running orders view
        public BarKitchenViewModel GetRunningOrdersViewModel()
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();
            List<Order> orders = _orderRepository.GetRunningOrders(menuItemType);

            return CreateViewModel(menuItemType, "running", false, orders);   //false=isFinishedView
        }

        // Get finished orders today view
        public BarKitchenViewModel GetFinishedOrdersTodayViewModel()
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();
            List<Order> orders = _orderRepository.GetFinishedOrdersToday(menuItemType);

            return CreateViewModel(menuItemType, "finished", true, orders);  
        }

        // Mark order item as being prepared
        public bool MarkItemBeingPrepared(int orderItemId)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            return _orderRepository.UpdateOrderItemStatus(
                orderItemId,
                menuItemType,
                OrderItemStatus.Ordered,
                OrderItemStatus.BeingPrepared);
        }

        // Mark order item as ready to be served
        public bool MarkItemReady(int orderItemId)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            return _orderRepository.UpdateOrderItemStatus(
                orderItemId,
                menuItemType,
                OrderItemStatus.BeingPrepared,
                OrderItemStatus.ReadyToBeServed);
        }

        // Mark all items in an order as being prepared
        public bool MarkOrderBeingPrepared(int orderId)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            return _orderRepository.UpdateOrderItemsStatusForOrder(
                orderId,
                menuItemType,
                OrderItemStatus.Ordered,
                OrderItemStatus.BeingPrepared);
        }

        // Mark all items in an order as ready to be served
        public bool MarkOrderReadyToServe(int orderId)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();

            return _orderRepository.UpdateAllOrderItemsToReady(orderId, menuItemType);
        }

        // Mark all items in a course as being prepared (Kitchen only)
        public bool MarkCourseBeingPrepared(int orderId, int courseType)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();
            
            // Only allow Kitchen staff to update courses
            if (menuItemType == MenuItemType.Food && IsValidCourseType(courseType))
            {
                return _orderRepository.UpdateCourseStatus(
                    orderId,
                    menuItemType,
                    (CourseType)courseType,
                    OrderItemStatus.Ordered,
                    OrderItemStatus.BeingPrepared);
            }

            return false;
        }

        // Mark all items in a course as ready to be served (Kitchen only)
        public bool MarkCourseReadyToServe(int orderId, int courseType)
        {
            MenuItemType menuItemType = GetMenuItemTypeForCurrentEmployee();
            
            // Only allow Kitchen to update courses
            if (menuItemType == MenuItemType.Food && IsValidCourseType(courseType))
            {
                return _orderRepository.UpdateCourseStatus(
                    orderId,
                    menuItemType,
                    (CourseType)courseType,
                    OrderItemStatus.BeingPrepared,
                    OrderItemStatus.ReadyToBeServed);
            }

            return false;
        }
        private bool IsValidCourseType(int courseType)
        {
            return Enum.IsDefined(typeof(CourseType), courseType)
                && (CourseType)courseType != CourseType.None;
        }

         private BarKitchenViewModel CreateViewModel(MenuItemType menuItemType,string viewMode,bool isFinishedView,List<Order> orders)
        {
            string pageTitle = menuItemType == MenuItemType.Food ? "Kitchen Orders" : "Bar Orders";

            if (isFinishedView)
                pageTitle += " - Finished Today";

            BarKitchenViewModel viewModel = new BarKitchenViewModel();
            viewModel.MenuItemType = menuItemType;
            viewModel.PageTitle = pageTitle;
            viewModel.ViewModel = viewMode;
            viewModel.IsFinishedView = isFinishedView;
            viewModel.Orders = orders;

            return viewModel;
        }
    }
}
