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
        public MetaExchangeController(IOrderBookManager orderBookManager)
        {
            _orderBookManager = orderBookManager;
        }

        [HttpPost]
        [Route("add-exchange-order")]
        public async Task<MetaOrder> AddNewExchangeOrder([FromBody]ExchangeOrder order)
        {
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
