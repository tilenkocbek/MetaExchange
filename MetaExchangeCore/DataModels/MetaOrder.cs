namespace MetaExchangeCore.DataModels
{
    public class MetaOrder
    {
        public long Id { get; set; }
        public long ExchangeOrderId { get; set; }
        public string ExchangeId { get; set; } 
        public DateTime Time { get; set; }
        public OrderType Type { get; set; }
        public OrderKind Kind { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal Price { get; set; }

        public static MetaOrder FromExchangeOrder(ExchangeOrder exchangeOrder)
        {
            return new MetaOrder
            {
                ExchangeOrderId = exchangeOrder.Id,
                ExchangeId = exchangeOrder.ExchangeId,
                Amount = exchangeOrder.Amount,
                Kind = exchangeOrder.Kind,
                Price = exchangeOrder.Price,
                RemainingAmount = exchangeOrder.RemainingAmount,
                Time = exchangeOrder.Time != DateTime.MinValue ? exchangeOrder.Time : DateTime.UtcNow,
                Type = exchangeOrder.Type
            };
        }
    }
}
