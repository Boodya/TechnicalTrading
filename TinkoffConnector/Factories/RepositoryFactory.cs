using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.Repositories;

namespace TinkoffConnector.Factories
{
    public static class RepositoryFactory
    {
        public static async Task<T> GetRepository<T>(IContext context, string accountId)
        {
            if(typeof(T) == typeof(InstrumentsRepository))
            {
                var instance = new InstrumentsRepository(context);
                await instance.InitializeInstruments();
                return (T)(object)instance;
            }
            else if(typeof(T) == typeof(CandlesRepository))
            {
                return (T)(object)new CandlesRepository(context, accountId);
            }
            else if (typeof(T) == typeof(LocalCandlesRepository))
            {
                return (T)(object)new LocalCandlesRepository(context, accountId);
            }
            else if (typeof(T) == typeof(OrdersRepository))
            {
                return (T)(object)new OrdersRepository(context, accountId);
            }
            else if (typeof(T) == typeof(LocalOrdersRepository))
            {
                return (T)(object)new LocalOrdersRepository(context, accountId);
            }
            else if (typeof(T) == typeof(PortfolioRepository))
            {
                return (T)(object)new PortfolioRepository(context, accountId);
            }
            else
            {
                throw new Exception("That type of repository not defined");
            }
        }
    }
}
