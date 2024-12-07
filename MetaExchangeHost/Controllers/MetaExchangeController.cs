using MetaExchangeCore;
using MetaExchangeCore.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace MetaExchangeHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetaExchangeController : ControllerBase
    {
        private readonly IOrderBookManager _orderBookManager;
        private readonly IExchangeManager _exchangeManager;
        public MetaExchangeController(IOrderBookManager orderBookManager, IExchangeManager exchangeManager)
        {
            _orderBookManager = orderBookManager;
            _exchangeManager = exchangeManager;
        }

        [HttpPost]
        [Route("add-exchange-order")]
        public async Task<MetaOrder> AddNewExchangeOrder([FromBody]ExchangeOrder order)
        {
            _exchangeManager.AddUpdateExchange(order.ExchangeId);
            return await _orderBookManager.AddExchangeOrder(order);
        }

        [HttpPost]
        [Route("add-user-order")]
        public async Task<AddUserOrderResponse> AddUserOrder([FromBody] AddUserOrder order)
        {
            return await _orderBookManager.HandleUserOrder(order);
        }

    }
}
