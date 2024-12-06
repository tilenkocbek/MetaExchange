using FluentAssertions;
using MetaExchangeCore;
using MetaExchangeCore.DataModels;

namespace MetaExchangeTests
{
    public class OrderBookTests
    {
        private readonly IOrderBook _orderBook;

        public OrderBookTests() {
            _orderBook = new OrderBook();
        }

        [Fact]
        public void TestOrderBook_AddBuyOrder()
        {
            MetaOrder buyOrder1 = new MetaOrder
            {
                Id = 1L,
                Amount = 1,
                RemainingAmount = 1,
                Price = 65_000.0m,
                Type = OrderType.Buy
            };

            _orderBook.AddOrder(buyOrder1).Should().BeTrue();
            //Adding it again with same orderId should fail!
            _orderBook.AddOrder(buyOrder1).Should().BeFalse();

            MetaOrder? bestBuyOrder = _orderBook.GetBestBuyOrder();
            bestBuyOrder.Should().NotBeNull();
            MetaOrder? bestSellOrder = _orderBook.GetBestSellOrder();
            bestSellOrder.Should().BeNull();

            bestBuyOrder!.Id.Should().Be(buyOrder1.Id);
            bestBuyOrder.Amount.Should().Be(buyOrder1.Amount);
            bestBuyOrder.RemainingAmount.Should().Be(buyOrder1.RemainingAmount);
            bestBuyOrder.Price.Should().Be(buyOrder1.Price);

            //When we add buy order with bigger price than previous best buy order, that order should become best buy order

            MetaOrder buyOrder2 = new MetaOrder
            {
                Id = buyOrder1.Id + 1,
                Amount = 2,
                RemainingAmount = 2,
                Price = 65_100.0m,
                Type = OrderType.Buy
            };

            _orderBook.AddOrder(buyOrder2).Should().BeTrue();
            bestBuyOrder = _orderBook.GetBestBuyOrder();
            bestBuyOrder.Should().NotBeNull();
            bestSellOrder = _orderBook.GetBestSellOrder();
            bestSellOrder.Should().BeNull();

            bestBuyOrder!.Id.Should().Be(buyOrder2.Id);
            bestBuyOrder.Amount.Should().Be(buyOrder2.Amount);
            bestBuyOrder.RemainingAmount.Should().Be(buyOrder2.RemainingAmount);
            bestBuyOrder.Price.Should().Be(buyOrder2.Price);

            //Add another order to an existing level
            MetaOrder buyOrder3 = new MetaOrder
            {
                Id = buyOrder2.Id + 1,
                Amount = 3,
                RemainingAmount = 3,
                Price = buyOrder2.Price,
                Type = OrderType.Buy
            };

            _orderBook.AddOrder(buyOrder3).Should().BeTrue();
            bestBuyOrder = _orderBook.GetBestBuyOrder();
            bestBuyOrder.Should().NotBeNull();

            bestBuyOrder!.Id.Should().Be(buyOrder2.Id);

            List<MetaOrder> allBuyOrders = _orderBook.GetAllBuyOrders().ToList();
            allBuyOrders.Should().HaveCount(3);
            allBuyOrders[0].Id.Should().Be(buyOrder2.Id);
            allBuyOrders[1].Id.Should().Be(buyOrder3.Id);
            allBuyOrders[2].Id.Should().Be(buyOrder1.Id);
        }

        [Fact]
        public void TestOrderBook_AddSellOrder()
        {
            MetaOrder sellOrder1 = new MetaOrder
            {
                Id = 1L,
                Amount = 1,
                RemainingAmount = 1,
                Price = 65_000.0m,
                Type = OrderType.Sell
            };

            _orderBook.AddOrder(sellOrder1).Should().BeTrue();
            //Adding it again with same orderId should fail!
            _orderBook.AddOrder(sellOrder1).Should().BeFalse();

            MetaOrder? bestSellOrder = _orderBook.GetBestSellOrder();
            bestSellOrder.Should().NotBeNull();
            MetaOrder? bestBuyOrder = _orderBook.GetBestBuyOrder();
            bestBuyOrder.Should().BeNull();

            bestSellOrder!.Id.Should().Be(sellOrder1.Id);
            bestSellOrder.Amount.Should().Be(sellOrder1.Amount);
            bestSellOrder.RemainingAmount.Should().Be(sellOrder1.RemainingAmount);
            bestSellOrder.Price.Should().Be(sellOrder1.Price);

            //When we add sell order with lesser price than previous best sell order, that order should become best buy order

            MetaOrder sellOrder2 = new MetaOrder
            {
                Id = sellOrder1.Id + 1,
                Amount = 2,
                RemainingAmount = 2,
                Price = 64_900.0m,
                Type = OrderType.Sell
            };

            _orderBook.AddOrder(sellOrder2).Should().BeTrue();
            bestBuyOrder = _orderBook.GetBestBuyOrder();
            bestBuyOrder.Should().BeNull();
            bestSellOrder = _orderBook.GetBestSellOrder();
            bestSellOrder.Should().NotBeNull();

            bestSellOrder!.Id.Should().Be(sellOrder2.Id);
            bestSellOrder.Amount.Should().Be(sellOrder2.Amount);
            bestSellOrder.RemainingAmount.Should().Be(sellOrder2.RemainingAmount);
            bestSellOrder.Price.Should().Be(sellOrder2.Price);

            //Add another order to an existing level
            MetaOrder sellOrder3 = new MetaOrder
            {
                Id = sellOrder2.Id + 1,
                Amount = 3,
                RemainingAmount = 3,
                Price = sellOrder2.Price,
                Type = OrderType.Sell
            };

            _orderBook.AddOrder(sellOrder3).Should().BeTrue();
            bestSellOrder = _orderBook.GetBestSellOrder();
            bestSellOrder.Should().NotBeNull();

            bestSellOrder!.Id.Should().Be(sellOrder2.Id);

            List<MetaOrder> allBuyOrders = _orderBook.GetAllSellOrders().ToList();
            allBuyOrders.Should().HaveCount(3);
            allBuyOrders[0].Id.Should().Be(sellOrder2.Id);
            allBuyOrders[1].Id.Should().Be(sellOrder3.Id);
            allBuyOrders[2].Id.Should().Be(sellOrder1.Id);
        }
    }
}
