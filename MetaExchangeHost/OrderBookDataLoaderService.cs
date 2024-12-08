
using MetaExchangeCore;
using MetaExchangeCore.Common;

namespace MetaExchangeHost
{
    public class OrderBookDataLoaderService : IHostedService
    {
        private readonly IOrderBookManager _orderBookManager;
        private readonly IExchangeManager _exchangeManager;
        private readonly ILogger<OrderBookDataLoaderService> _logger;
        public OrderBookDataLoaderService(ILogger<OrderBookDataLoaderService> logger, IOrderBookManager obm, IExchangeManager exchManager)
        {
            _logger = logger;
            _orderBookManager = obm;
            _exchangeManager = exchManager;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!OrderBookDataLoader.OrderBookDataFileExists())
            {
                throw new Exception($"File with order book data not found at path {OrderBookDataLoader.OrderBookFilePath}");
            }
            var stats = await OrderBookDataLoader.ImportOrderBookDataFile(_orderBookManager, _exchangeManager);

            _logger.LogInformation($"Setup of order book data finished!");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // nop
            return Task.CompletedTask;
        }
    }
}
