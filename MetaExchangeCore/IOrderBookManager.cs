using MetaExchangeCore.DataModels;

namespace MetaExchangeCore
{
    public interface IOrderBookManager
    {
        Task<MetaOrder> AddExchangeOrder(ExchangeOrder exchangeOrder);
        Task<AddUserOrderResponse> HandleUserOrder(AddUserOrder userOrder);
        void SubscribeToOrderUpdates(OrderUpdated hdl);
    }
}
