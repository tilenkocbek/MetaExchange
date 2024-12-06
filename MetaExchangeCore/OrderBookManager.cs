using MetaExchangeCore.DataModels;
using System.Collections.Concurrent;

namespace MetaExchangeCore
{
    public class OrderBookManager : IOrderBookManager
    {
        private readonly IOrderBook _orderBook;
        //Do we even need this
        private readonly ConcurrentDictionary<long, MetaOrder> _orders = new();

        private long _nextOrderId = 0L;
        public OrderBookManager()
        {
            _orderBook = new OrderBook();
        }

        /// <summary>
        /// This handles orders that come from other exchanges - it maps the order into our 'internal' order object and adds it to order book.
        /// </summary>
        public MetaOrder AddExchangeOrder(ExchangeOrder exchangeOrder)
        {
            if (!IsOrderValid(exchangeOrder))
            {
                //Order from exchange would get validated in ExchangeClient as soon as we receive that order from some other exchange.
                //Instead of throwing exception, we would set the status of ExchangeOrder to 'Failed' or something like that and push that to stream
                throw new Exception($"Exchange Order with id {exchangeOrder.Id} from exchange {exchangeOrder.ExchangeId} is not valid");
            }
            MetaOrder order = new MetaOrder
            {
                Id = Interlocked.Increment(ref _nextOrderId),
                ExchangeOrderId = exchangeOrder.Id,
                ExchangeId = exchangeOrder.ExchangeId,
                Amount = exchangeOrder.Amount,
                Kind = exchangeOrder.Kind,
                Price = exchangeOrder.Price,
                RemainingAmount = exchangeOrder.RemainingAmount,
                Time = DateTime.UtcNow,
                Type = exchangeOrder.Type
            };

            //TODO: Lock here

            if (!_orderBook.AddOrder(order))
            {
                throw new Exception($"Could not add order {exchangeOrder.Id} from exchange {exchangeOrder.ExchangeId} to order book.");
            }

            _orders.TryAdd(order.Id, order);
            return order;
        }

        public IEnumerable<OrderTrade> HandleUserOrder(AddUserOrder userOrder)
        {
            if (!IsOrderValid(userOrder))
            {
                throw new Exception($"User Order for amount {userOrder.Amount} and type {userOrder.OrderType} is not valid");
            }

            //Check if there even is a market, if there is no market, return some sort of status - probably need to modify what is returned here.
            //E.g. return UserOrderResponse that will have list of ORderTrades and also executedAmount, remaining Amt, avgPrice, etc.
            //Do we need to validate if it can be filled before looping? Or can it be done inside loop?

            //TODO: Locking should be introduced here when we handle the order after validation


            //While loop, getting best bids/asks to get to the desired qty
            //Look at OrderBookNew.GetBestSellOrders
            throw new NotImplementedException();
        }

        private OrderTrade BuildTrade(AddUserOrder userOrder, MetaOrder bookOrder)
        {
            return null;
        }

        private bool IsOrderValid(AddUserOrder order)
        {
            if (order.OrderType == OrderType.Unknown || order.Amount <= decimal.Zero)
                return false;
            return true;
        }

        private bool IsOrderValid(ExchangeOrder order)
        {
            if (string.IsNullOrWhiteSpace(order.ExchangeId) ||
                order.Type == OrderType.Unknown ||
                order.Kind == OrderKind.Unknown ||
                order.Price <= decimal.Zero ||
                order.Amount <= decimal.Zero)
                return false;
            return true;
        }
    }
}
