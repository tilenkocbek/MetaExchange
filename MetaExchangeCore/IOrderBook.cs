using MetaExchangeCore.DataModels;

namespace MetaExchangeCore
{
    /// <summary>
    /// This should be per product pair, so each product pair would have its own order book.
    /// 
    /// </summary>
    public interface IOrderBook
    {
        decimal BestBid();
        decimal BestAsk();
        bool RemoveOrder(MetaOrder order);
        bool AddOrder(MetaOrder order);
        IEnumerable<MetaOrder> GetAllSellOrders();
        IEnumerable<MetaOrder> GetAllBuyOrders();
        IEnumerable<MetaOrder> GetBestSellOrders();
        IEnumerable<MetaOrder> GetBestBuyOrders();
    }
}
