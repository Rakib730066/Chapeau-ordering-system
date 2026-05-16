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

        public BarKitchenService(IBarKitchenRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // Get Kitchen ViewModel with food orders
        public BarKitchenViewModel GetKitchenViewModel()
        {
            BarKitchenViewModel viewModel = new BarKitchenViewModel();
            viewModel.PageTitle = "Kitchen Orders";
            viewModel.ReturnPage = "Kitchen";
            viewModel.MenuItemType = MenuItemType.Food;
            viewModel.Orders = _orderRepository.GetRunningOrders(MenuItemType.Food);

            return viewModel;
        }

        // Get Bar ViewModel with drink orders
        public BarKitchenViewModel GetBarViewModel()
        {
            BarKitchenViewModel viewModel = new BarKitchenViewModel();
            viewModel.PageTitle = "Bar Orders";
            viewModel.ReturnPage = "Bar";
            viewModel.MenuItemType = MenuItemType.Drink;
            viewModel.Orders = _orderRepository.GetRunningOrders(MenuItemType.Drink);

            return viewModel;
        }

        // Mark order item as being prepared
        public void MarkItemBeingPrepared(int orderItemId)
        {
            _orderRepository.UpdateOrderItemStatus(
                orderItemId,
                MenuItemType.Food,
                OrderItemStatus.Ordered,
                OrderItemStatus.BeingPrepared);
        }

        // Mark order item as ready to be served
        public void MarkItemReady(int orderItemId)
        {
            _orderRepository.UpdateOrderItemStatus(
                orderItemId,
                MenuItemType.Food,
                OrderItemStatus.BeingPrepared,
                OrderItemStatus.ReadyToBeServed);
        }
    }
}
