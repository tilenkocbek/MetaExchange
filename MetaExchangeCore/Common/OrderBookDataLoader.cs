using System.Text.Json.Serialization;
using System.Text.Json;
using MetaExchangeCore.DataModels;

namespace MetaExchangeCore.Common
{
    public static class OrderBookDataLoader
    {
        public static string OrderBookFilePath = Path.Combine(AppContext.BaseDirectory, "OrderBookData", "order_books_data_simple.txt");
        public static bool OrderBookDataFileExists()
        {
            if (!File.Exists(OrderBookFilePath))
            {
                return false;
            }
            return true;
        }

        public static async Task<ImportOrderBookStats> ImportOrderBookDataFile(IOrderBookManager orderBookManager, IExchangeManager exchangeManager)
        {
            if (!OrderBookDataFileExists())
                throw new Exception($"File with order book data not found at path {OrderBookFilePath}");
            ImportOrderBookStats stats = new ImportOrderBookStats();

            int lineCnt = 0;
            foreach (var line in File.ReadLines(OrderBookFilePath))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] splitted = line.Split("\t");
                    if (splitted.Length != 2)
                    {
                        throw new Exception($"Could not parse string at line {lineCnt}. {line}");
                    }
                    string exchangeId = $"Exchange-{stats.Exchanges}";
                    exchangeManager.AddUpdateExchange(exchangeId);
                    OrderBookEntries? exchangeOrderBook = JsonSerializer.Deserialize<OrderBookEntries>(splitted[1], new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    });
                    if (exchangeOrderBook == null)
                    {
                        throw new Exception($"Could not deserialize order book at line {lineCnt}");
                    }

                    foreach (var bidOrder in exchangeOrderBook.Bids)
                    {
                        var exchangeOrder = OrderBookOrder.ToExchangeOrder(bidOrder.Order);
                        exchangeOrder.ExchangeId = exchangeId;
                        await orderBookManager.AddExchangeOrder(exchangeOrder);
                        stats.BuyOrders++;
                        stats.BuyAmount += exchangeOrder.Amount;
                    }

                    foreach (var askOrder in exchangeOrderBook.Asks)
                    {
                        var exchangeOrder = OrderBookOrder.ToExchangeOrder(askOrder.Order);
                        exchangeOrder.ExchangeId = exchangeId;
                        await orderBookManager.AddExchangeOrder(exchangeOrder);
                        stats.SellOrders++;
                        stats.SellAmount += exchangeOrder.Amount;
                    }

                    stats.Exchanges++;
                }
            }

            return stats;
        }

        private class OrderBookEntries
        {
            public List<OrderBookEntry> Bids { get; set; } = [];
            public List<OrderBookEntry> Asks { get; set; } = [];
        }

        private class OrderBookEntry
        {
            public OrderBookOrder Order { get; set; }
        }

        private class OrderBookOrder
        {
            public long? Id { get; set; }
            public DateTime Time { get; set; }
            public OrderType Type { get; set; }
            public OrderKind Kind { get; set; }
            public decimal Amount { get; set; }
            public decimal Price { get; set; }

            public static ExchangeOrder ToExchangeOrder(OrderBookOrder orderBookOrder)
            {
                return new ExchangeOrder
                {
                    Id = orderBookOrder.Id ?? new Random().NextInt64() + DateTime.UtcNow.Ticks,
                    Amount = orderBookOrder.Amount,
                    RemainingAmount = orderBookOrder.Amount,
                    Price = orderBookOrder.Price,
                    Kind = orderBookOrder.Kind,
                    Time = orderBookOrder.Time,
                    Type = orderBookOrder.Type
                };
            }
        }
    }

    public class ImportOrderBookStats
    {
        public int SellOrders { get; set; }
        public decimal SellAmount { get; set; }
        public int BuyOrders { get; set; }
        public decimal BuyAmount { get; set; }
        public int Exchanges { get; set; }
    }

}
