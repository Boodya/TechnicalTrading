using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.Factories;
using TinkoffConnector.Repositories;

namespace TinkoffConnector.Connectors
{
    public abstract class TinkoffConnectorBase : ITinkoffConnector
    {
        public Currency MainCurrency { get; protected set; }
        public IInstrumentsRepository Instruments { get; protected set; }
        public ICandlesRepository Candles { get; protected set; }
        public IOrdersRepository Orders { get; protected set; }
        public IPortfolioRepository Portfolio { get; protected set; }
        public readonly Context context;
        protected string accountId;

        public TinkoffConnectorBase(string authToken)
        {
            var connection = Tinkoff.Trading.OpenApi.Network.ConnectionFactory.GetSandboxConnection(authToken);
            context = connection.Context;
            MainCurrency = Currency.Rub;
        }

        public virtual Task RegisterAccount()
        {
            throw new NotImplementedException();
        }

        protected virtual async void InitializeRepositories()
        {
            Instruments = await RepositoryFactory.GetRepository<InstrumentsRepository>(context, accountId);
            Candles = await RepositoryFactory.GetRepository<CandlesRepository>(context, accountId);
            Orders = await RepositoryFactory.GetRepository<OrdersRepository>(context, accountId);
            Portfolio = await RepositoryFactory.GetRepository<PortfolioRepository>(context, accountId);
        }
    }
}
