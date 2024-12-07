using MetaExchangeCore.DataModels;

namespace MetaExchangeCore
{
    public delegate void OrderUpdated(MetaOrder order);
    public class ExchangeManager : IExchangeManager
    {
        private readonly IOrderBookManager _orderBookManager;
        private HashSet<string> exchanges = [];
        public ExchangeManager(IOrderBookManager orderBookManager)
        {
            _orderBookManager = orderBookManager;
            _orderBookManager.SubscribeToOrderUpdates(OrderUpdateHandler);
        }
        public void AddUpdateExchange(string exchangeId) => exchanges.Add(exchangeId);

        private void OrderUpdateHandler(MetaOrder order)
        {
            if(exchanges.Contains(order.ExchangeId))
            {
                //Map it to exchange order, send order update with appropriate status 
                // to source exchange
            }
        }
    }
}
