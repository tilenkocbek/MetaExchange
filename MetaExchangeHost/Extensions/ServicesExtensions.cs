using MetaExchangeCore;

namespace MetaExchangeHost.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddMetaExchangeServices(this IServiceCollection services)
        {
            services.AddSingleton<IOrderBook, OrderBook>();
            services.AddSingleton<IOrderBookManager, OrderBookManager>();
            services.AddSingleton<IExchangeManager, ExchangeManager>();
            services.AddHostedService<OrderBookDataLoaderService>();
            return services;
        }
    }
}
