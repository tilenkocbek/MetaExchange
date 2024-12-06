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
    }
}
