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
        private long _nextTradeId = 0L;
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
            MetaOrder order = MetaOrder.FromExchangeOrder(exchangeOrder);
            order.Id = Interlocked.Increment(ref _nextOrderId);

            //TODO: Lock here

            if (!_orderBook.AddOrder(order))
            {
                throw new Exception($"Could not add order {exchangeOrder.Id} from exchange {exchangeOrder.ExchangeId} to order book.");
            }

            _orders.TryAdd(order.Id, order);
            return order;
        }

        public AddUserOrderResponse HandleUserOrder(AddUserOrder userOrder)
        {
            if (!IsOrderValid(userOrder))
            {
                throw new Exception($"User Order for amount {userOrder.Amount} and type {userOrder.OrderType} is not valid");
            }

            if (userOrder.OrderType == OrderType.Sell)
                return HandleSellUserOrder(userOrder);
            return HandleBuyUserOrder(userOrder);
        }

        private AddUserOrderResponse HandleSellUserOrder(AddUserOrder userOrder)
        {
            AddUserOrderResponse response = new AddUserOrderResponse(userOrder);
            response.Id = Interlocked.Increment(ref _nextOrderId);

            //TODO: Locking should be introduced here when we handle the order after validation
            if (_orderBook.GetBestBuyOrder() == null)
            {
                //There is no buy market, we can't do anything
                response.Status = UserOrderStatus.Cancelled;
                response.StatusChangeReason = StatusChangeReason.NoMarket;
                return response;
            }

            while (response.ExecutedAmount < response.OriginalAmount && _orderBook.GetBestBuyOrder() != null)
            {

            }

            return response;
        }

        private AddUserOrderResponse HandleBuyUserOrder(AddUserOrder userOrder)
        {
            AddUserOrderResponse response = new AddUserOrderResponse(userOrder);
            response.Id = Interlocked.Increment(ref _nextOrderId);

            //TODO: Locking should be introduced here when we handle the order after validation
            if (_orderBook.GetBestSellOrder() == null)
            {
                //There is no sell market, we can't do anything
                response.Status = UserOrderStatus.Cancelled;
                response.StatusChangeReason = StatusChangeReason.NoMarket;
                return response;
            }

            while (response.RemainingAmount > decimal.Zero && _orderBook.GetBestSellOrder() != null)
            {
                MetaOrder bookOrder = _orderBook.GetBestSellOrder()!;
                OrderTrade trade = BuildTrade(response, bookOrder);
                if (trade != null)
                {
                    response.Trades.Add(trade);
                }

                if (bookOrder.RemainingAmount <= decimal.Zero)
                {
                    if (!_orderBook.RemoveOrder(bookOrder))
                    {
                        throw new Exception("Something went very wrong");
                    }
                }

                //TODO: Send update of book order to exchange 
            }


            //Check if fully executed, if it is, appropriate status set
            if (response.RemainingAmount == decimal.Zero)
            {
                response.Status = UserOrderStatus.FullyExecuted;
            }
            else if (response.ExecutedAmount > decimal.Zero)
            {
                response.Status = UserOrderStatus.PartiallyExecuted;
            }
            return response;
        }

        private OrderTrade BuildTrade(AddUserOrderResponse userOrder, MetaOrder bookOrder)
        {
            OrderTrade trade = new OrderTrade
            {
                OrderId = userOrder.Id,
                TradeId = Interlocked.Increment(ref _nextTradeId),
                Amount = Math.Min(userOrder.RemainingAmount, bookOrder.RemainingAmount),
                Price = bookOrder.Price,
                OrderType = userOrder.OrderType,
            };

            userOrder.ExecutedAmount += trade.Amount;
            bookOrder.RemainingAmount -= trade.Amount;

            //TODO: Output the trade or somehow update that bookOrder has changed and notify exchange where that order came from

            return trade;
        }

        private static bool IsOrderValid(AddUserOrder order)
        {
            if (order.OrderType == OrderType.Unknown || order.Amount <= decimal.Zero)
                return false;
            return true;
        }

        private static bool IsOrderValid(ExchangeOrder order)
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
