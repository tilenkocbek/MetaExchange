using MetaExchangeCore.DataModels;

namespace MetaExchangeCore
{
    /// <summary>
    /// This should be per product pair, so each product pair would have its own order book.
    /// 
    /// </summary>
    public interface IOrderBook
    {
        MetaOrder? GetBestBuyOrder();
        MetaOrder? GetBestSellOrder();
        bool RemoveOrder(MetaOrder order);
        bool AddOrder(MetaOrder order);
        IEnumerable<MetaOrder> GetAllSellOrders();
        IEnumerable<MetaOrder> GetAllBuyOrders();
    }
}
