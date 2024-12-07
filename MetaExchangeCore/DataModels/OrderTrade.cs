namespace MetaExchangeCore.DataModels
{
    public class OrderTrade
    {
        public long OrderId { get; set; }
        public long ExchangeOrderId { get; set; }
        public string ExchangeId { get; set; }  
        public long TradeId { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public OrderType OrderType { get; set; }
        public decimal Value => Price * Amount;
    }
}
