namespace MetaExchangeCore.DataModels
{
    public record AddUserOrder
    {
        public OrderType OrderType { get; set; }
        public decimal Amount { get; set; }
    }
}
