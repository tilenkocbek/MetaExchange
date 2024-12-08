using MetaExchangeCore;
using MetaExchangeCore.Common;
using MetaExchangeCore.DataModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaExchange
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (!OrderBookDataLoader.OrderBookDataFileExists())
            {
                Console.WriteLine($"File with order book data not found at path {OrderBookDataLoader.OrderBookFilePath}");
                return;
            } 

            OrderBookManager orderBookManager = new OrderBookManager(new OrderBook());
            ExchangeManager exchangeManager = new ExchangeManager(orderBookManager);

            var stats = await OrderBookDataLoader.ImportOrderBookDataFile(orderBookManager, exchangeManager);
            Console.WriteLine($"Added {stats.BuyOrders} Buy orders and {stats.SellOrders} Sell orders from {stats.Exchanges} exchanges");
            Console.WriteLine($"Sum amount of all buy orders is {stats.BuyAmount} BTC, while sum amount of all sell orders is {stats.SellAmount}");

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
                Console.WriteLine($"\n");
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
                Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters =
                            {
                                new JsonStringEnumConverter()
                            }
                }));
            };
        }
    }
}
