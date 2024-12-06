using MetaExchangeCore.DataModels;
using System.Collections.Concurrent;

namespace MetaExchangeCore
{
    public class OrderBook : IOrderBook
    {
        // <orderId, MetaOrder>
        private readonly ConcurrentDictionary<long, MetaOrder> _orders = new();
        // <price, List of orders for that price>
        private readonly SortedDictionary<decimal, LinkedList<MetaOrder>> _buys = new SortedDictionary<decimal, LinkedList<MetaOrder>>(new DescendingComparer<decimal>());
        private readonly SortedDictionary<decimal, LinkedList<MetaOrder>> _sells = [];
        private MetaOrder? _bestBuyOrder;
        private MetaOrder? _bestSellOrder;

        //Just for listing purposes - do we even need this?
        public IEnumerable<MetaOrder> GetAllBuyOrders() => _buys.Values.SelectMany(x => x.ToList());

        public IEnumerable<MetaOrder> GetAllSellOrders() => _sells.Values.SelectMany(x => x.ToList());
        public MetaOrder? GetBestBuyOrder() => _bestBuyOrder;

        public MetaOrder? GetBestSellOrder() => _bestSellOrder;

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
            _sells.Add(order.Price, orders);

            if (_bestSellOrder == null || order.Price < _bestSellOrder.Price)
            {
                //Since this price is lower than best ask price, we need to update the best ask price
                _bestSellOrder = order;
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

            if (_bestBuyOrder == null || order.Price > _bestBuyOrder.Price)
            {
                //Since this price is bigger than best bid price, we need to update the best bid price
                _bestBuyOrder = order;
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

            if (order == _bestSellOrder)
            {
                LinkedListNode<MetaOrder> node = orders.First!;
                _bestSellOrder = node.Next?.Value;
            }
            if (!orders.Remove(order))
                return false;

            if (orders.Count == 0)
            {
                //No more orders for this level, remove the price level
                _sells.Remove(order.Price);
                if (_sells.Count > 0)
                {
                    _bestSellOrder = _sells.First().Value.First();
                }
            }
            return true;
        }

        private bool RemoveBuyOrder(MetaOrder order)
        {
            if (!_buys.TryGetValue(order.Price, out var orders))
            {
                //Should not happen, order book is in bad state
                return false;
            }

            if (order == _bestBuyOrder)
            {
                LinkedListNode<MetaOrder> node = orders.First!;
                _bestBuyOrder = node.Next?.Value;
            }
            if (!orders.Remove(order))
                return false;

            if (orders.Count == 0)
            {
                //No more orders for this level, remove the price level
                _buys.Remove(order.Price);
                if (_buys.Count > 0)
                {
                    _bestBuyOrder = _buys.First().Value.First();
                }
            }
            return true;
        }

        private class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }
    }
}
