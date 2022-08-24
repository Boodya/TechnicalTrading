using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.Repositories;

namespace TinkoffConnector.Connectors
{
    public interface ITinkoffConnector
    {
        public Currency MainCurrency { get; }
        public IInstrumentsRepository Instruments { get; }
        public ICandlesRepository Candles { get; }
        public IOrdersRepository Orders { get; }
        public IPortfolioRepository Portfolio { get; }
        public Task RegisterAccount();
    }
}
