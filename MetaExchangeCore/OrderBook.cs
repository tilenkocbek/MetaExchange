using MetaExchangeCore.DataModels;
using System.Collections.Concurrent;

namespace MetaExchangeCore
{
    internal class OrderBook : IOrderBook
    {
        // <orderId, MetaOrder>
        private readonly ConcurrentDictionary<long, MetaOrder> _orders = new();
        // <price, List of orders for that price>
        private readonly SortedDictionary<decimal, LinkedList<MetaOrder>> _buys = [];
        private readonly SortedDictionary<decimal, LinkedList<MetaOrder>> _sells = [];
        private decimal _bestBidPrice;
        private decimal _bestAskPrice;

        public decimal BestAsk() => _bestAskPrice;

        public decimal BestBid() => _bestBidPrice;

        //Just for listing purposes - do we even need this?
        public IEnumerable<MetaOrder> GetAllBuyOrders() => _buys.Values.SelectMany(x => x.ToList());

        public IEnumerable<MetaOrder> GetAllSellOrders() => _sells.Values.SelectMany(x => x.ToList());

        public bool AddOrder(MetaOrder order)
        {
            if (order.Type == OrderType.Buy)
            {
                return AddBuyOrder(order);
            }
            else if (order.Type == OrderType.Sell)
            {
                return AddSellOrder(order);
            }
            return false;
        }

        private bool AddSellOrder(MetaOrder order)
        {
            if (!_orders.TryAdd(order.Id, order))
                return false;
            if (_sells.TryGetValue(order.Price, out var orders))
            {
                //We already have this price, add this order to the end of the list for this price  
                orders.AddLast(order);
                return true;
            }

            orders = [];
            orders.AddLast(order);
            //Create a new price level
            _buys.Add(order.Price, orders);

            if(order.Price < _bestAskPrice)
            {
                //Since this price is lower than best ask price, we need to update the best ask price
                _bestAskPrice = order.Price;
            }
            return true;
        }

        private bool AddBuyOrder(MetaOrder order)
        {
            if (!_orders.TryAdd(order.Id, order))
                return false;
            if (_buys.TryGetValue(order.Price, out var orders))
            {
                //We already have this price, add this order to the end of the list for this price  
                orders.AddLast(order);
                return true;
            }

            orders = [];
            orders.AddLast(order);
            //Create a new price level
            _buys.Add(order.Price, orders);

            if (order.Price > _bestBidPrice)
            {
                //Since this price is bigger than best bid price, we need to update the best bid price
                _bestBidPrice = order.Price;
            }
            return true;
        }

        public bool RemoveOrder(MetaOrder order)
        {
            if (order.Type == OrderType.Buy)
            {
                return RemoveBuyOrder(order);
            }
            else if (order.Type == OrderType.Sell)
            {
                return RemoveSellOrder(order);
            }
            return false;
        }

        private bool RemoveSellOrder(MetaOrder order)
        {
            if (!_sells.TryGetValue(order.Price, out var orders))
            {
                //Should not happen, order book is in bad state
                return false;
            }

            return orders.Remove(order);
        }

        private bool RemoveBuyOrder(MetaOrder order)
        {
            if (!_buys.TryGetValue(order.Price, out var orders))
            {
                //Should not happen, order book is in bad state
                return false;
            }
            return orders.Remove(order);
        }


        //TODO: This needs locking of some sort
        public IEnumerable<MetaOrder> GetBestSellOrders()
        {
            if (_sells.Count == 0)
                yield break;

            foreach (var sellPriceLevel in _sells)
            {
                LinkedList<MetaOrder> orders = sellPriceLevel.Value;
                foreach (var order in orders)
                    yield return order;
            }
            yield break;
        }

        //TODO: This needs locking of some sort
        public IEnumerable<MetaOrder> GetBestBuyOrders()
        {
            throw new NotImplementedException();
        }
    }
}
