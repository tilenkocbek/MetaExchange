using MetaExchangeCore.DataModels;

namespace MetaExchangeCore
{
    public interface IOrderBookManager
    {
        MetaOrder AddExchangeOrder(ExchangeOrder exchangeOrder);
        AddUserOrderResponse HandleUserOrder(AddUserOrder userOrder);
    }
}
