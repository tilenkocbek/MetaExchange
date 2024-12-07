namespace MetaExchangeCore.DataModels
{
    public class OrderNotValidException : Exception
    {
        public OrderNotValidException(string msg) : base(msg) { }
    }
}
