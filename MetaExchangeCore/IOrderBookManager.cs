using MetaExchangeCore.DataModels;

namespace MetaExchangeCore
{
    public interface IOrderBookManager
    {
        MetaOrder AddExchangeOrder(ExchangeOrder exchangeOrder);
        IEnumerable<OrderTrade> HandleUserOrder(AddUserOrder userOrder);
    }
}
