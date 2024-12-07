using FluentAssertions;
using MetaExchangeCore;
using MetaExchangeCore.DataModels;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace MetaExchangeTests
{
    public class OrderBookManagerTests
    {
        private readonly IOrderBook _orderBook;
        private readonly OrderBookManager _orderBookManager;
        public OrderBookManagerTests()
        {
            _orderBook = Substitute.For<IOrderBook>();
            _orderBookManager = new OrderBookManager(_orderBook);
        }

        [Fact]
        public void AddExchangeOrder_InvalidOrderShouldThrow()
        {
            ExchangeOrder exchangeOrder = new ExchangeOrder()
            {
                ExchangeId = Guid.NewGuid().ToString(),
                Amount = 1.5m,
                Id = 100_000,
                Kind = OrderKind.Market,
                Price = 65_000.0m,
                RemainingAmount = 1.5m,
                Time = DateTime.UtcNow,
                Type = OrderType.Buy
            };

            //invalid exchangeId
            exchangeOrder.ExchangeId = null;
            Action act = () => _orderBookManager.AddExchangeOrder(exchangeOrder);
            act.Should().Throw<OrderNotValidException>();

            exchangeOrder.ExchangeId = "";
            act = () => _orderBookManager.AddExchangeOrder(exchangeOrder);
            act.Should().Throw<OrderNotValidException>();

            exchangeOrder.ExchangeId = "  ";
            act = () => _orderBookManager.AddExchangeOrder(exchangeOrder);
            act.Should().Throw<OrderNotValidException>();

            //invalid order type
            exchangeOrder.ExchangeId = Guid.NewGuid().ToString();
            exchangeOrder.Type = OrderType.Unknown;
            act = () => _orderBookManager.AddExchangeOrder(exchangeOrder);
            act.Should().Throw<OrderNotValidException>();

            //invalid kind
            exchangeOrder.Type = OrderType.Buy;
            exchangeOrder.Kind = OrderKind.Unknown;
            act = () => _orderBookManager.AddExchangeOrder(exchangeOrder);
            act.Should().Throw<OrderNotValidException>();

            //invalid price
            exchangeOrder.Kind = OrderKind.Market;
            exchangeOrder.Price = decimal.Zero;
            act = () => _orderBookManager.AddExchangeOrder(exchangeOrder);
            act.Should().Throw<OrderNotValidException>();

            //invalid amount
            exchangeOrder.Price = 0.01m;
            exchangeOrder.Amount = decimal.Zero;
            act = () => _orderBookManager.AddExchangeOrder(exchangeOrder);
            act.Should().Throw<OrderNotValidException>();
        }

        [Fact]
        public void AddExchangeOrder_BadOrderBook()
        {
            ExchangeOrder exchangeOrder = new ExchangeOrder()
            {
                ExchangeId = Guid.NewGuid().ToString(),
                Amount = 1.5m,
                Id = 100_000,
                Kind = OrderKind.Market,
                Price = 65_000.0m,
                RemainingAmount = 1.5m,
                Time = DateTime.UtcNow,
                Type = OrderType.Buy
            };

            _orderBook.AddOrder(Arg.Is<MetaOrder>(o => o.ExchangeOrderId == exchangeOrder.Id)).Returns(false);

            Action act = () => _orderBookManager.AddExchangeOrder(exchangeOrder);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddExchangeOrder_Success()
        {
            ExchangeOrder exchangeOrder = new ExchangeOrder()
            {
                ExchangeId = Guid.NewGuid().ToString(),
                Amount = 1.5m,
                Id = 100_000,
                Kind = OrderKind.Market,
                Price = 65_000.0m,
                RemainingAmount = 1.5m,
                Time = DateTime.UtcNow,
                Type = OrderType.Buy
            };

            MetaOrder? createdMetaOrder = null;
            _orderBook.AddOrder(Arg.Do<MetaOrder>(o => createdMetaOrder = o)).Returns(true);

            _orderBookManager.AddExchangeOrder(exchangeOrder);

            createdMetaOrder.Should().NotBeNull();
            createdMetaOrder!.Id.Should().Be(1L);
            createdMetaOrder.ExchangeId.Should().Be(exchangeOrder.ExchangeId);
            createdMetaOrder.Amount.Should().Be(exchangeOrder.Amount);
            createdMetaOrder.ExchangeOrderId.Should().Be(exchangeOrder.Id);
            createdMetaOrder.Kind.Should().Be(exchangeOrder.Kind);
            createdMetaOrder.Price.Should().Be(exchangeOrder.Price);
            createdMetaOrder.RemainingAmount.Should().Be(exchangeOrder.RemainingAmount);
            createdMetaOrder.Time.Should().Be(exchangeOrder.Time);
            createdMetaOrder.Type.Should().Be(exchangeOrder.Type);
        }

        [Fact]
        public void AddUserOrder_InvalidOrderShouldThrow()
        {
            AddUserOrder userOrder = new AddUserOrder()
            {
                Amount = 1.5m,
                OrderType = OrderType.Buy
            };

            //invalid type
            userOrder.OrderType = OrderType.Unknown;
            Action act = () => _orderBookManager.HandleUserOrder(userOrder);
            act.Should().Throw<OrderNotValidException>();

            userOrder.OrderType = OrderType.Buy;
            userOrder.Amount = decimal.Zero;
            act = () => _orderBookManager.HandleUserOrder(userOrder);
            act.Should().Throw<OrderNotValidException>();
        }

        [Fact]
        public void AddUserOrderBuy_NoSellMarket()
        {
            AddUserOrder userOrder = new AddUserOrder()
            {
                Amount = 1.5m,
                OrderType = OrderType.Buy
            };

            _orderBook.GetBestSellOrder().Returns((MetaOrder?)null);

            AddUserOrderResponse resp = _orderBookManager.HandleUserOrder(userOrder);
            resp.Should().NotBeNull();
            resp.Id.Should().Be(1);
            resp.Status.Should().Be(UserOrderStatus.Cancelled);
            resp.StatusChangeReason.Should().Be(StatusChangeReason.NoMarket);
        }

        [Fact]
        public void AddUserOrderSell_NoBuyMarket()
        {
            AddUserOrder userOrder = new AddUserOrder()
            {
                Amount = 1.5m,
                OrderType = OrderType.Sell
            };

            _orderBook.GetBestBuyOrder().Returns((MetaOrder?)null);

            AddUserOrderResponse resp = _orderBookManager.HandleUserOrder(userOrder);
            resp.Should().NotBeNull();
            resp.Id.Should().Be(1);
            resp.Status.Should().Be(UserOrderStatus.Cancelled);
            resp.StatusChangeReason.Should().Be(StatusChangeReason.NoMarket);
        }

        [Fact]
        public void AddUserOrderBuy_ExecutePartially()
        {
            AddUserOrder userOrder = new AddUserOrder()
            {
                Amount = 1.5m,
                OrderType = OrderType.Buy
            };

            MetaOrder bestSellOrder = new MetaOrder
            {
                Id = 123,
                ExchangeId = Guid.NewGuid().ToString(),
                ExchangeOrderId = 1234,
                Amount = 2.0m,
                Price = 50_000.05m,
                RemainingAmount = 1.0m,
            };

            _orderBook.GetBestSellOrder().Returns(bestSellOrder, bestSellOrder, bestSellOrder, (MetaOrder?)null);
            _orderBook.RemoveOrder(Arg.Any<MetaOrder>()).Returns(true);

            AddUserOrderResponse resp = _orderBookManager.HandleUserOrder(userOrder);
            resp.Should().NotBeNull();
            resp.Id.Should().Be(1);
            resp.Status.Should().Be(UserOrderStatus.PartiallyExecuted);
            resp.OrderType.Should().Be(userOrder.OrderType);
            resp.OriginalAmount.Should().Be(userOrder.Amount);
            resp.ExecutedAmount.Should().Be(1.0m);
            resp.RemainingAmount.Should().Be(0.5m);
            resp.Value.Should().Be(1.0m * bestSellOrder.Price);
            resp.AveragePrice.Should().Be(bestSellOrder.Price);
            resp.Trades.Should().HaveCount(1);
            resp.Trades[0].OrderId.Should().Be(1);
            resp.Trades[0].TradeId.Should().Be(1);
            resp.Trades[0].ExchangeId.Should().Be(bestSellOrder.ExchangeId);
            resp.Trades[0].ExchangeOrderId.Should().Be(bestSellOrder.ExchangeOrderId);
            resp.Trades[0].OrderId.Should().Be(1);
            resp.Trades[0].Amount.Should().Be(1.0m);
            resp.Trades[0].Price.Should().Be(bestSellOrder.Price);
            resp.Trades[0].OrderType.Should().Be(userOrder.OrderType);

            _orderBook.Received(1).RemoveOrder(Arg.Is<MetaOrder>(o => o.Id == bestSellOrder.Id));
        }

        [Fact]
        public void AddUserOrderBuy_ExecuteFromMultipleOrders()
        {
            AddUserOrder userOrder = new AddUserOrder()
            {
                Amount = 1.5m,
                OrderType = OrderType.Buy
            };

            MetaOrder bestSellOrder = new MetaOrder
            {
                Id = 123,
                ExchangeId = Guid.NewGuid().ToString(),
                ExchangeOrderId = 1234,
                Amount = 2.0m,
                Price = 50_000.05m,
                RemainingAmount = 1.0m,
            };

            MetaOrder bestSellOrder2 = new MetaOrder
            {
                Id = 123,
                ExchangeId = Guid.NewGuid().ToString(),
                ExchangeOrderId = 12345,
                Amount = 2.0m,
                Price = 55_000.05m,
                RemainingAmount = 2.0m,
            };

            _orderBook.GetBestSellOrder().Returns(bestSellOrder, bestSellOrder, bestSellOrder, bestSellOrder2, bestSellOrder2, (MetaOrder?)null);
            _orderBook.RemoveOrder(Arg.Any<MetaOrder>()).Returns(true);

            AddUserOrderResponse resp = _orderBookManager.HandleUserOrder(userOrder);
            resp.Should().NotBeNull();
            resp.Id.Should().Be(1);
            resp.Status.Should().Be(UserOrderStatus.FullyExecuted);
            resp.OrderType.Should().Be(userOrder.OrderType);
            resp.OriginalAmount.Should().Be(userOrder.Amount);
            resp.ExecutedAmount.Should().Be(userOrder.Amount);
            resp.RemainingAmount.Should().Be(decimal.Zero);
            resp.Value.Should().Be(1.0m * bestSellOrder.Price + 0.5m * bestSellOrder2.Price);
            resp.AveragePrice.Should().Be(((bestSellOrder.Price + (0.5m * bestSellOrder2.Price)) / 1.5m));
            resp.Trades.Should().HaveCount(2);
            resp.Trades[0].OrderId.Should().Be(1);
            resp.Trades[0].TradeId.Should().Be(1);
            resp.Trades[0].ExchangeId.Should().Be(bestSellOrder.ExchangeId);
            resp.Trades[0].ExchangeOrderId.Should().Be(bestSellOrder.ExchangeOrderId);
            resp.Trades[0].Amount.Should().Be(1.0m);
            resp.Trades[0].Price.Should().Be(bestSellOrder.Price);
            resp.Trades[0].OrderType.Should().Be(userOrder.OrderType);
            resp.Trades[1].OrderId.Should().Be(1);
            resp.Trades[1].TradeId.Should().Be(2);
            resp.Trades[1].ExchangeId.Should().Be(bestSellOrder2.ExchangeId);
            resp.Trades[1].ExchangeOrderId.Should().Be(bestSellOrder2.ExchangeOrderId);
            resp.Trades[1].Amount.Should().Be(0.5m);
            resp.Trades[1].Price.Should().Be(bestSellOrder2.Price);
            resp.Trades[1].OrderType.Should().Be(userOrder.OrderType);
            _orderBook.Received(1).RemoveOrder(Arg.Is<MetaOrder>(o => o.Id == bestSellOrder.Id));
            _orderBook.Received(1).RemoveOrder(Arg.Is<MetaOrder>(o => o.Id == bestSellOrder2.Id));

        }

        [Fact]
        public void AddUserOrderSell_ExecutePartially()
        {
            AddUserOrder userOrder = new AddUserOrder()
            {
                Amount = 1.5m,
                OrderType = OrderType.Sell
            };

            MetaOrder bestBuyOrder = new MetaOrder
            {
                Id = 123,
                ExchangeId = Guid.NewGuid().ToString(),
                ExchangeOrderId = 1234,
                Amount = 2.0m,
                Price = 50_000.05m,
                RemainingAmount = 1.0m,
            };

            _orderBook.GetBestBuyOrder().Returns(bestBuyOrder, bestBuyOrder, bestBuyOrder, (MetaOrder?)null);
            _orderBook.RemoveOrder(Arg.Any<MetaOrder>()).Returns(true);

            AddUserOrderResponse resp = _orderBookManager.HandleUserOrder(userOrder);
            resp.Should().NotBeNull();
            resp.Id.Should().Be(1);
            resp.Status.Should().Be(UserOrderStatus.PartiallyExecuted);
            resp.OrderType.Should().Be(userOrder.OrderType);
            resp.OriginalAmount.Should().Be(userOrder.Amount);
            resp.ExecutedAmount.Should().Be(1.0m);
            resp.RemainingAmount.Should().Be(0.5m);
            resp.Value.Should().Be(1.0m * bestBuyOrder.Price);
            resp.AveragePrice.Should().Be(bestBuyOrder.Price);
            resp.Trades.Should().HaveCount(1);
            resp.Trades[0].OrderId.Should().Be(1);
            resp.Trades[0].TradeId.Should().Be(1);
            resp.Trades[0].ExchangeId.Should().Be(bestBuyOrder.ExchangeId);
            resp.Trades[0].ExchangeOrderId.Should().Be(bestBuyOrder.ExchangeOrderId);
            resp.Trades[0].OrderId.Should().Be(1);
            resp.Trades[0].Amount.Should().Be(1.0m);
            resp.Trades[0].Price.Should().Be(bestBuyOrder.Price);
            resp.Trades[0].OrderType.Should().Be(userOrder.OrderType);

            _orderBook.Received(1).RemoveOrder(Arg.Is<MetaOrder>(o => o.Id == bestBuyOrder.Id));
        }

        [Fact]
        public void AddUserOrderSell_ExecuteFromMultipleOrders()
        {
            AddUserOrder userOrder = new AddUserOrder()
            {
                Amount = 1.5m,
                OrderType = OrderType.Sell
            };

            MetaOrder bestBuyOrder = new MetaOrder
            {
                Id = 123,
                ExchangeId = Guid.NewGuid().ToString(),
                ExchangeOrderId = 1234,
                Amount = 2.0m,
                Price = 50_000.05m,
                RemainingAmount = 1.0m,
            };

            MetaOrder bestBuyOrder2 = new MetaOrder
            {
                Id = 123,
                ExchangeId = Guid.NewGuid().ToString(),
                ExchangeOrderId = 12345,
                Amount = 2.0m,
                Price = 51_000.95m,
                RemainingAmount = 2.0m,
            };

            _orderBook.GetBestBuyOrder().Returns(bestBuyOrder, bestBuyOrder, bestBuyOrder, bestBuyOrder2, bestBuyOrder2, (MetaOrder?)null);
            _orderBook.RemoveOrder(Arg.Any<MetaOrder>()).Returns(true);

            AddUserOrderResponse resp = _orderBookManager.HandleUserOrder(userOrder);
            resp.Should().NotBeNull();
            resp.Id.Should().Be(1);
            resp.Status.Should().Be(UserOrderStatus.FullyExecuted);
            resp.OrderType.Should().Be(userOrder.OrderType);
            resp.OriginalAmount.Should().Be(userOrder.Amount);
            resp.ExecutedAmount.Should().Be(userOrder.Amount);
            resp.RemainingAmount.Should().Be(decimal.Zero);
            resp.Value.Should().Be(1.0m * bestBuyOrder.Price + 0.5m * bestBuyOrder2.Price);
            resp.AveragePrice.Should().Be(((bestBuyOrder.Price + (0.5m * bestBuyOrder2.Price)) / 1.5m));
            resp.Trades.Should().HaveCount(2);
            resp.Trades[0].OrderId.Should().Be(1);
            resp.Trades[0].TradeId.Should().Be(1);
            resp.Trades[0].ExchangeId.Should().Be(bestBuyOrder.ExchangeId);
            resp.Trades[0].ExchangeOrderId.Should().Be(bestBuyOrder.ExchangeOrderId);
            resp.Trades[0].Amount.Should().Be(1.0m);
            resp.Trades[0].Price.Should().Be(bestBuyOrder.Price);
            resp.Trades[0].OrderType.Should().Be(userOrder.OrderType);
            resp.Trades[1].OrderId.Should().Be(1);
            resp.Trades[1].TradeId.Should().Be(2);
            resp.Trades[1].ExchangeId.Should().Be(bestBuyOrder2.ExchangeId);
            resp.Trades[1].ExchangeOrderId.Should().Be(bestBuyOrder2.ExchangeOrderId);
            resp.Trades[1].Amount.Should().Be(0.5m);
            resp.Trades[1].Price.Should().Be(bestBuyOrder2.Price);
            resp.Trades[1].OrderType.Should().Be(userOrder.OrderType);
            _orderBook.Received(1).RemoveOrder(Arg.Is<MetaOrder>(o => o.Id == bestBuyOrder.Id));
            _orderBook.Received(1).RemoveOrder(Arg.Is<MetaOrder>(o => o.Id == bestBuyOrder2.Id));

        }
    }
}
