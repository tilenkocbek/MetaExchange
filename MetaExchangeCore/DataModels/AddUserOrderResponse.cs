namespace MetaExchangeCore.DataModels
{
    public class AddUserOrderResponse
    {
        public AddUserOrderResponse(AddUserOrder userOrder)
        {
            OriginalAmount = userOrder.Amount;
            OrderType = userOrder.OrderType;
        }
        public long Id { get; set; }
        public OrderType OrderType { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal ExecutedAmount { get; set; }
        public decimal RemainingAmount => OriginalAmount - ExecutedAmount;
        public decimal AveragePrice => Trades.Any() ? Trades.Sum(x => x.Value) / Trades.Sum(x => x.Amount) : decimal.Zero;
        public decimal Value => ExecutedAmount * AveragePrice;
        public UserOrderStatus Status { get; set; } = UserOrderStatus.InProgress;
        public StatusChangeReason StatusChangeReason { get; set; }
        public List<OrderTrade> Trades { get; set; } = [];
    }
}
