using MetaExchangeCore;
using MetaExchangeCore.DataModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaExchange
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "OrderBookData", "order_books_data_simple.txt");
            if (!File.Exists(path))
            {
                Console.WriteLine($"File with order book data not found at path {path}");
                return;
            }

            OrderBookManager orderBookManager = new OrderBookManager(new OrderBook());
            ExchangeManager exchangeManager = new ExchangeManager(orderBookManager);
            int sellCnt = 0;
            int buyCnt = 0;
            decimal sellAmount = 0;
            decimal buyAmt = 0;
            int exchanges = 0;
            int lineCnt = 1;
            //Read file and populate order book with data
            foreach (var line in File.ReadLines(path))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] splitted = line.Split("\t");
                    if (splitted.Length != 2)
                    {
                        Console.WriteLine($"Could not parse string at line {lineCnt}. {line}");
                        return;
                    }
                    string exchangeId = $"Exchange-{exchanges}";
                    exchangeManager.AddUpdateExchange(exchangeId);
                    OrderBookEntries? exchangeOrderBook = JsonSerializer.Deserialize<OrderBookEntries>(splitted[1], new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    });
                    if (exchangeOrderBook == null)
                    {
                        Console.WriteLine($"Could not deserialize order book at line {lineCnt}");
                        return;
                    }

                    foreach (var bidOrder in exchangeOrderBook.Bids)
                    {
                        var exchangeOrder = OrderBookOrder.ToExchangeOrder(bidOrder.Order);
                        exchangeOrder.ExchangeId = exchangeId;
                        await orderBookManager.AddExchangeOrder(exchangeOrder);
                        buyCnt++;
                        buyAmt += exchangeOrder.Amount;
                    }

                    foreach (var askOrder in exchangeOrderBook.Asks)
                    {
                        var exchangeOrder = OrderBookOrder.ToExchangeOrder(askOrder.Order);
                        exchangeOrder.ExchangeId = exchangeId;
                        await orderBookManager.AddExchangeOrder(exchangeOrder);
                        sellCnt++;
                        sellAmount += exchangeOrder.Amount;
                    }

                    exchanges++;
                }
            }

            Console.WriteLine($"Added {buyCnt} Buy orders and {sellCnt} Sell orders from {exchanges} exchanges");
            Console.WriteLine($"Sum amount of all buy orders is {buyAmt} BTC, while sum amount of all sell orders is {sellAmount}");

            Console.WriteLine("\b \b ------------------------------------ SETUP FINISHED -----------------");
            //Wait for user input
            //Make sure to not close the console after user inputs the data
            bool cancelPressed = false;
            Console.CancelKeyPress += (_, ea) =>
            {
                cancelPressed = true;
                ea.Cancel = true;
            };

            while (!cancelPressed)
            {
                Console.WriteLine($"\n \n ");
                OrderType orderType = OrderType.Unknown;
                while (orderType == OrderType.Unknown)
                {
                    Console.WriteLine("Would you like to buy or sell?");
                    string typeString = Console.ReadLine()!;
                    if (typeString.Equals(OrderType.Buy.ToString(), StringComparison.InvariantCultureIgnoreCase) || typeString.Equals("b", StringComparison.InvariantCultureIgnoreCase))
                    {
                        orderType = OrderType.Buy;
                    }
                    else if (typeString.Equals(OrderType.Sell.ToString(), StringComparison.InvariantCultureIgnoreCase) || typeString.Equals("s", StringComparison.InvariantCultureIgnoreCase))
                    {
                        orderType = OrderType.Sell;
                    }
                    else
                    {
                        Console.WriteLine("Unknown value - valid values are 'buy', 'b, 'sell', 's'");
                    }
                }
                decimal amount = decimal.Zero;
                while (amount == decimal.Zero)
                {
                    Console.WriteLine("How much would you like to buy or sell?");
                    string amountString = Console.ReadLine()!;
                    if (decimal.TryParse(amountString, out decimal parsedAmount) && parsedAmount > decimal.Zero)
                    {
                        amount = parsedAmount;
                    }
                    else
                    {
                        Console.WriteLine($"Please input a number that is greater than 0!");
                    }
                }

                AddUserOrder userOrder = new AddUserOrder { Amount = amount, OrderType = orderType };
                AddUserOrderResponse response = await orderBookManager.HandleUserOrder(userOrder);
                Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true,
                    Converters =
                            {
                                new JsonStringEnumConverter()
                            }
                }));
            };
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
}
